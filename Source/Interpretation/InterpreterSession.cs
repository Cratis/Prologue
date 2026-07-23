// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents an <see cref="IInterpreterSession"/> — deterministic heuristics build the model's structure, and an
/// optional language model refines the names, derives the system name, describes every module, feature, and slice,
/// and may ask the user questions when genuinely uncertain. All conversation state lives in the serializable
/// <see cref="InterpreterSessionState"/> and the language model is stateless, so any host can persist the state at
/// a checkpoint and resume later by replaying the transcript. A language model that fails, times out, or responds
/// unusably never fails the session — the deterministic heuristic model is always the result floor.
/// </summary>
/// <param name="state">The state to start from — fresh for a new session, previously serialized for a resumed one.</param>
/// <param name="captures">The correlated captures the session interprets.</param>
/// <param name="llmOptions">The language-model configuration; refinement is skipped entirely when disabled.</param>
/// <param name="heuristics">The deterministic model builder.</param>
/// <param name="chatClients">The factory creating the chat client when refinement runs.</param>
/// <param name="logger">The logger.</param>
/// <param name="statusChanged">An optional callback invoked on every status transition.</param>
/// <param name="maxQuestionRounds">The maximum number of ask-and-answer rounds before the session finalizes with whatever the model last returned; zero disallows questions entirely.</param>
public class InterpreterSession(
    InterpreterSessionState state,
    IReadOnlyList<Capture> captures,
    LlmOptions llmOptions,
    IBuildHeuristicModel heuristics,
    IChatClientFactory chatClients,
    ILogger<InterpreterSession> logger,
    Action<InterpreterStatus>? statusChanged,
    int maxQuestionRounds) : IInterpreterSession
{
    IChatClient? _chatClient;

    /// <inheritdoc/>
    public InterpreterSessionState State { get; private set; } = state;

    /// <inheritdoc/>
    public async Task<InterpreterSessionState> Proceed(CancellationToken cancellationToken = default)
    {
        switch (State.Status)
        {
            case InterpreterStatus.Completed or InterpreterStatus.Failed or InterpreterStatus.GeneratingScreenplay:
                return State;
            case InterpreterStatus.AwaitingAnswers when State.PendingQuestions.Count > 0:
                return State;
            case InterpreterStatus.AwaitingAnswers:
                return await ContinueRefinement(cancellationToken);
            default:
                return await Start(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public InterpreterSessionState Answer(InterpreterAnswer answer)
    {
        var question = State.PendingQuestions.FirstOrDefault(pending => pending.Id == answer.QuestionId)
            ?? throw new QuestionNotPending(answer.QuestionId);

        State = State with
        {
            PendingQuestions = [.. State.PendingQuestions.Where(pending => pending.Id != question.Id)],
            AnsweredQuestions = [.. State.AnsweredQuestions, new AnsweredQuestion(question, answer.Value)]
        };

        return State;
    }

    async Task<InterpreterSessionState> Start(CancellationToken cancellationToken)
    {
        try
        {
            Transition(InterpreterStatus.ReadingCaptures);
            Transition(InterpreterStatus.AnalyzingEvidence);
            Transition(InterpreterStatus.BuildingModel);
            var heuristic = State.Model ?? heuristics.Build(State.PrologueId, captures);
            State = State with { Model = heuristic };

            // A model without modules gives the language model nothing to refine, so it finalizes exactly like a
            // disabled language model does — deterministically.
            if (!llmOptions.Enabled || heuristic.Modules.Count == 0)
            {
                return Finalize(heuristic);
            }

            Transition(InterpreterStatus.Refining);
            if (State.Transcript.Count == 0)
            {
                Append(ChatRole.System, RefinementPrompt.System(maxQuestionRounds > 0));
                Append(ChatRole.User, RefinementPrompt.Evidence(heuristic, captures));
            }

            return await ExchangeWithModel(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            InterpreterSessionLog.SessionFailed(logger, exception);
            State = State with { Error = exception.Message };
            Transition(InterpreterStatus.Failed);
            return State;
        }
    }

    async Task<InterpreterSessionState> ContinueRefinement(CancellationToken cancellationToken)
    {
        Transition(InterpreterStatus.Refining);
        Append(ChatRole.User, RefinementPrompt.Answers(LastRoundAnswers()));
        State = State with { QuestionRounds = State.QuestionRounds + 1 };
        return await ExchangeWithModel(cancellationToken);
    }

    async Task<InterpreterSessionState> ExchangeWithModel(CancellationToken cancellationToken)
    {
        var heuristic = State.Model!;
        try
        {
            // Bound the exchange to its own budget so a slow or hung model falls back to the heuristic model
            // instead of blocking the session. A cancellation from the caller's token is a real cancel; one from
            // the budget is a graceful fallback.
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            budget.CancelAfter(llmOptions.RefinementTimeout);

            _chatClient ??= chatClients.CreateFor(llmOptions);
            var response = await _chatClient.GetResponseAsync(
                [.. State.Transcript.Select(message => new ChatMessage(new ChatRole(message.Role), message.Text))],
                new ChatOptions { Temperature = 0.2f, ModelId = LlmChatClient.EffectiveModelId(llmOptions) },
                budget.Token);

            Append(ChatRole.Assistant, response.Text);

            var refinement = ModelRefinementParser.Parse(response.Text);
            if (refinement is null)
            {
                InterpreterSessionLog.UnparsableRefinement(logger);
                return Finalize(heuristic);
            }

            if (refinement.Questions.Count > 0 && State.QuestionRounds < maxQuestionRounds)
            {
                State = State with
                {
                    PendingQuestions =
                    [
                        .. refinement.Questions.Select(question =>
                            new InterpreterQuestion(Guid.NewGuid(), question.Prompt, question.Choices, question.Context))
                    ]
                };
                Transition(InterpreterStatus.AwaitingAnswers);
                return State;
            }

            return Finalize(ModelRenamer.Apply(heuristic, refinement));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            InterpreterSessionLog.RefinementFailed(logger, exception);
            return Finalize(heuristic);
        }
    }

    InterpreterSessionState Finalize(ExtractionResult model)
    {
        if (model.SystemName.Length == 0)
        {
            model = model with { SystemName = SystemNameDeriver.Derive(model) };
        }

        State = State with { Model = SliceOrdering.Order(model), PendingQuestions = [] };
        Transition(InterpreterStatus.Completed);
        return State;
    }

    IReadOnlyList<AnsweredQuestion> LastRoundAnswers()
    {
        // The questions of the round being answered live in the transcript's last assistant message — the session
        // only entered AwaitingAnswers after successfully parsing it, so parsing it again attributes the round's
        // answers without carrying extra bookkeeping in the serialized state.
        var lastAssistant = State.Transcript.LastOrDefault(message => message.Role == ChatRole.Assistant.Value);
        var asked = lastAssistant is null ? 0 : ModelRefinementParser.Parse(lastAssistant.Text)?.Questions.Count ?? 0;
        var count = asked == 0 ? State.AnsweredQuestions.Count : Math.Min(asked, State.AnsweredQuestions.Count);

        return [.. State.AnsweredQuestions.TakeLast(count)];
    }

    void Append(ChatRole role, string text) =>
        State = State with { Transcript = [.. State.Transcript, new SessionChatMessage(role.Value, text)] };

    void Transition(InterpreterStatus status)
    {
        State = State with { Status = status };
        statusChanged?.Invoke(status);
    }
}
