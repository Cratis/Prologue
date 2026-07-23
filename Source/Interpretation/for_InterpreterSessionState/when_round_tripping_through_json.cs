// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_InterpreterSessionState;

public class when_round_tripping_through_json : Specification
{
    static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    InterpreterSessionState _state;
    InterpreterSessionState _deserialized;

    void Establish()
    {
        var pending = new InterpreterQuestion(
            Guid.NewGuid(),
            "Is an author the same as a writer?",
            [new QuestionChoice("Yes", "They are the same concept"), new QuestionChoice("No", string.Empty)],
            "The evidence uses both terms");
        var answered = new AnsweredQuestion(
            new InterpreterQuestion(Guid.NewGuid(), "What does the system manage?", [], "The domain is unclear"),
            "A public library");
        var model = new ExtractionResult(
            _prologueId,
            [
                new ExtractedModule(
                    "Library",
                    [
                        new ExtractedFeature(
                            "Authors",
                            [],
                            [
                                new ExtractedSlice(
                                    "Register",
                                    ExtractedSliceType.StateChange,
                                    [new ExtractedCommand("RegisterAuthor", [new ExtractedProperty("Name", "string", IsRequired: true, MaxLength: 200)], [new ExtractedValidationRule("Name", ExtractedValidationRuleKind.MaxLength, "200", "Name is too long")])],
                                    [new ExtractedEvent("AuthorRegistered", [new ExtractedProperty("Name", "string")])],
                                    [],
                                    [],
                                    [new ExtractedConstraint("UniqueName", "name", "AuthorRegistered")],
                                    "Registers an author in the library")
                            ],
                            "Everything about authors")
                    ],
                    "The library domain")
            ],
            "LibrarySystem");

        _state = new InterpreterSessionState(
            _prologueId,
            InterpreterStatus.AwaitingAnswers,
            model,
            [pending],
            [answered],
            [new SessionChatMessage("system", "instructions"), new SessionChatMessage("user", "evidence"), new SessionChatMessage("assistant", "{ \"questions\": [] }")],
            1,
            string.Empty);
    }

    void Because() => _deserialized = JsonSerializer.Deserialize<InterpreterSessionState>(JsonSerializer.Serialize(_state, _json), _json)!;

    [Fact] void should_keep_the_prologue_id() => _deserialized.PrologueId.ShouldEqual(_prologueId);
    [Fact] void should_keep_the_status() => _deserialized.Status.ShouldEqual(InterpreterStatus.AwaitingAnswers);
    [Fact] void should_keep_the_question_rounds() => _deserialized.QuestionRounds.ShouldEqual(1);
    [Fact] void should_keep_the_error() => _deserialized.Error.ShouldEqual(string.Empty);
    [Fact] void should_keep_the_pending_question_identity() => _deserialized.PendingQuestions.Single().Id.ShouldEqual(_state.PendingQuestions[0].Id);
    [Fact] void should_keep_the_pending_question_prompt() => _deserialized.PendingQuestions.Single().Prompt.ShouldEqual("Is an author the same as a writer?");
    [Fact] void should_keep_the_pending_question_context() => _deserialized.PendingQuestions.Single().Context.ShouldEqual("The evidence uses both terms");
    [Fact] void should_keep_the_choices_with_their_descriptions() => _deserialized.PendingQuestions.Single().Choices.Single(choice => choice.Label == "Yes").Description.ShouldEqual("They are the same concept");
    [Fact] void should_keep_the_answered_question_prompt() => _deserialized.AnsweredQuestions.Single().Question.Prompt.ShouldEqual("What does the system manage?");
    [Fact] void should_keep_the_answer() => _deserialized.AnsweredQuestions.Single().Answer.ShouldEqual("A public library");
    [Fact] void should_keep_the_transcript_in_order() => string.Join('|', _deserialized.Transcript.Select(message => message.Role)).ShouldEqual("system|user|assistant");
    [Fact] void should_keep_the_transcript_text() => _deserialized.Transcript[2].Text.ShouldEqual("{ \"questions\": [] }");
    [Fact] void should_keep_the_model_system_name() => _deserialized.Model!.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_keep_the_module_description() => _deserialized.Model!.Modules.Single().Description.ShouldEqual("The library domain");
    [Fact] void should_keep_the_slice_description() => Slice.Description.ShouldEqual("Registers an author in the library");
    [Fact] void should_keep_the_nested_property_details() => Slice.Commands.Single().Properties.Single().MaxLength.ShouldEqual(200);
    [Fact] void should_keep_the_validation_rule_kind() => Slice.Commands.Single().Validations.Single().Kind.ShouldEqual(ExtractedValidationRuleKind.MaxLength);
    [Fact] void should_keep_the_constraint() => Slice.Constraints.Single().OnEvent.ShouldEqual("AuthorRegistered");

    ExtractedSlice Slice => _deserialized.Model!.Modules.Single().Features.Single().Slices.Single();
}
#endif
