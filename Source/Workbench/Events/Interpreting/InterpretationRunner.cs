// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Commands;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Uncaptured;
using Cratis.Prologue.Workbench.Prologues;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Represents an <see cref="IInterpretationRunner"/> that reuses the Interpretation library's session as it is: the heuristic model builder, the
/// language-model refinement through the chat client factory it is given, and the questions. What it adds is the Workbench's evidence, the scope
/// chosen in Explore, and recording only what differs from the last interpretation.
/// </summary>
/// <param name="evidence">Gathers the captures and the Workbench's evidence.</param>
/// <param name="heuristics">The deterministic model builder.</param>
/// <param name="chatClients">Creates the chat client refinement talks through; backed by Cratis.AI in the Workbench.</param>
/// <param name="sessions">Holds interpretations that wait for answers.</param>
/// <param name="progress">Holds how the interpretations are going.</param>
/// <param name="pipeline">Executes the command that records the result.</param>
/// <param name="loggers">Creates the loggers sessions log through.</param>
/// <param name="logger">The logger.</param>
public class InterpretationRunner(
    IEvidenceSource evidence,
    IBuildHeuristicModel heuristics,
    IChatClientFactory chatClients,
    IInterpretationSessions sessions,
    IInterpretationProgress progress,
    ICommandPipeline pipeline,
    ILoggerFactory loggers,
    ILogger<InterpretationRunner> logger) : IInterpretationRunner
{
    const int MaxQuestionRounds = 3;

    static readonly LlmOptions _options = new() { Enabled = true, ModelId = "cratis-ai", RefinementTimeout = TimeSpan.FromMinutes(5) };

    /// <summary>
    /// Says what an interpretation found and, when the language model could not be used, why the names are the mechanical ones.
    /// </summary>
    /// <param name="suggestions">How many events were suggested.</param>
    /// <param name="refined">Whether the language model refined the result.</param>
    /// <param name="failure">Why the language model could not be reached; empty when there is no reason to give.</param>
    /// <returns>The sentence.</returns>
    public static string Describe(int suggestions, bool refined, string failure)
    {
        var found = $"{suggestions} suggested event{(suggestions == 1 ? string.Empty : "s")}";
        return refined || failure.Length == 0 ? found : $"{found}. The Interpreter agent could not be used: {failure}";
    }

    /// <inheritdoc/>
    public async Task Run(InterpretationJob job, CancellationToken cancellationToken)
    {
        try
        {
            await (job.Resume ? Resume(job.Prologue, cancellationToken) : Start(job.Prologue, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Crashed(job.Prologue.Value, exception);
            Report(job.Prologue, InterpretationState.Failed, "The interpretation could not finish", false, 0);
        }
    }

    async Task Start(PrologueId prologue, CancellationToken cancellationToken)
    {
        Report(prologue, InterpretationState.Running, "Reading the captures");
        var input = await evidence.Load(prologue, cancellationToken);
        var included = ScopedCaptures.Included(input.Captures, input.Scope);
        if (included.All(capture => ScopedCaptures.ShapeOf(capture).IsNone))
        {
            Report(prologue, InterpretationState.Failed, "There are no captures of commands to interpret yet. Capture some traffic first, or bring excluded shapes back in Explore", false, 0);
            return;
        }

        var builder = new ScopedHeuristicModelBuilder(heuristics, input.Scope);
        var context = new InterpretationContext(included, ScopedCaptures.Partition(included, input.Scope), builder.Build(prologue.Value, included), input.Facts, input.Scope, input.Answers);
        var session = Factory(builder).CreateNew(prologue.Value, included, _options, status => Report(prologue, status), MaxQuestionRounds, EvidenceText.For(input.Facts));
        await Drive(prologue, session, context, cancellationToken);
    }

    async Task Resume(PrologueId prologue, CancellationToken cancellationToken)
    {
        var parked = sessions.Take(prologue);
        if (parked is null)
        {
            Report(prologue, InterpretationState.Failed, "No interpretation waits for answers. Interpret again", false, 0);
            return;
        }

        Report(prologue, InterpretationState.Running, "Carrying on with your answers");
        var builder = new ScopedHeuristicModelBuilder(heuristics, parked.Context.Scope);
        var session = Factory(builder).Resume(parked.State, parked.Context.Captures, _options, status => Report(prologue, status), MaxQuestionRounds);
        await Drive(prologue, session, parked.Context, cancellationToken);
    }

    async Task Drive(PrologueId prologue, IInterpreterSession session, InterpretationContext context, CancellationToken cancellationToken)
    {
        var state = await session.Proceed(cancellationToken);
        while (state.Status == InterpreterStatus.AwaitingAnswers && state.PendingQuestions.Count > 0)
        {
            var known = context.Answers.Concat(state.AnsweredQuestions.Select(answered => new QuestionAnswer(QuestionKeys.For(answered.Question.Prompt), answered.Question.Prompt, answered.Answer))).ToList();
            foreach (var question in state.PendingQuestions)
            {
                if (QuestionKeys.AnswerTo(question, known) is { } answer)
                {
                    state = session.Answer(new InterpreterAnswer(question.Id, answer.Answer));
                }
            }

            if (state.PendingQuestions.Count > 0)
            {
                sessions.Park(prologue, new ParkedInterpretation(state, context));
                Report(prologue, InterpretationState.AwaitingAnswers, state.PendingQuestions.Count == 1 ? "1 question from the interpreter waits for you" : $"{state.PendingQuestions.Count} questions from the interpreter wait for you");
                return;
            }

            state = await session.Proceed(cancellationToken);
        }

        await Finish(prologue, state, context);
    }

    async Task Finish(PrologueId prologue, InterpreterSessionState state, InterpretationContext context)
    {
        if (state.Status == InterpreterStatus.Failed)
        {
            Report(prologue, InterpretationState.Failed, state.Error.Length > 0 ? state.Error : "The interpreter failed", false, 0);
            return;
        }

        var (captured, heuristicModel) = SuggestionExtraction.From(context.Partitions, context.Heuristic, state.Model, state.Renames ?? new Dictionary<string, string>(), context.Facts);
        var (suggestions, model, questions) = UncapturedEvents.Extend(captured, heuristicModel, context.Facts, context.Answers);
        var boundaries = BoundaryDetection.Detect(context.Facts, context.Scope);
        var notEvents = NotEventDetection.Detect(context.Facts, context.Scope);
        var result = await pipeline.Execute(new RecordInterpretation(prologue, suggestions, model, boundaries, notEvents, questions));
        if (!result.IsSuccess)
        {
            Report(prologue, InterpretationState.Failed, result.ValidationResults.FirstOrDefault()?.Message ?? "Recording the interpretation failed", false, 0);
            return;
        }

        var refined = state.Renames is not null;
        var failure = (chatClients as ILanguageModelWatch)?.Failure ?? string.Empty;
        Report(prologue, InterpretationState.Completed, Describe(suggestions.Count, refined, failure), refined, suggestions.Count);
    }

    InterpreterSessionFactory Factory(IBuildHeuristicModel builder) => new(builder, chatClients, loggers.CreateLogger<InterpreterSession>());

    void Report(PrologueId prologue, InterpreterStatus status)
    {
        var message = status switch
        {
            InterpreterStatus.ReadingCaptures => "Reading the captures",
            InterpreterStatus.AnalyzingEvidence => "Analyzing the evidence",
            InterpreterStatus.BuildingModel => "Building the model",
            InterpreterStatus.Refining => "Asking the Interpreter agent to name what it found",
            _ => string.Empty
        };
        if (message.Length > 0)
        {
            Report(prologue, InterpretationState.Running, message);
        }
    }

    void Report(PrologueId prologue, InterpretationState state, string message) =>
        Report(prologue, state, message, progress.Get(prologue).Refined, progress.Get(prologue).Suggestions);

    void Report(PrologueId prologue, InterpretationState state, string message, bool refined, int suggestions) =>
        progress.Set(new InterpretationProgress(prologue, state, message, refined, suggestions));
}
