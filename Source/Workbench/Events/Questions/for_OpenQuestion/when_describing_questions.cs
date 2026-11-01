// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Questions.for_OpenQuestion;

public class when_describing_questions : a_library_interpretation
{
    static readonly InterpreterQuestion _about = new(Guid.NewGuid(), "Is the AuthorCreated event the same as registering a writer?", [new QuestionChoice("Yes", "Merge them"), new QuestionChoice("No", "")], "Authors are inserted by one command");
    static readonly InterpreterQuestion _aboutNothing = new(Guid.NewGuid(), "Is the library open on Sundays?", [], "Nobody knows");
    IEnumerable<OpenQuestion> _questions;
    IEnumerable<EventCard> _cards;
    IEnumerable<EventCard> _cardsWithoutQuestions;
    IEnumerable<EventCard> _cardsOfANameChange;

    void Because()
    {
        _questions = OpenQuestion.From([_about, _aboutNothing], _review);
        _cards = EventCard.From(_review, [_about]);
        _cardsWithoutQuestions = EventCard.From(_review, []);
        _cardsOfANameChange = EventCard.From(EventReview.Reduce(new EventSuggestionRenamed("Authors/AuthorCreated", "AuthorRegistration"), _review, _context), []);
    }

    [Fact] void should_describe_each_question() => _questions.Count().ShouldEqual(2);
    [Fact] void should_key_a_question_by_its_prompt() => _questions.First().Key.ShouldEqual(QuestionKeys.For(_about.Prompt));
    [Fact] void should_pass_the_choices_on() => _questions.First().Choices.Select(choice => choice.Label).ShouldContainOnly("Yes", "No");
    [Fact] void should_say_which_suggestions_the_question_is_about() => _questions.First().Affects.ShouldContain((SuggestionKey)"Authors/AuthorCreated");
    [Fact] void should_not_link_a_question_that_is_about_nothing_suggested() => _questions.Last().Affects.ShouldBeEmpty();
    [Fact] void should_say_which_events_need_an_answer() => _cards.Single(card => card.Key.Value == "Authors/AuthorCreated").NeedsAnswer.ShouldBeTrue();
    [Fact] void should_not_say_it_about_the_others() => _cards.Count(card => card.NeedsAnswer).ShouldEqual(1);
    [Fact] void should_need_no_answer_when_no_question_waits() => _cardsWithoutQuestions.Any(card => card.NeedsAnswer).ShouldBeFalse();
    [Fact] void should_not_need_an_answer_once_the_event_is_decided() => EventCard.From(EventReview.Reduce(new EventSuggestionAccepted("Authors/AuthorCreated"), _review, _context), [_about]).Any(card => card.NeedsAnswer).ShouldBeFalse();
    [Fact] void should_advise_about_a_name_that_is_not_past_tense() => _cardsOfANameChange.Single(card => card.Key.Value == "Authors/AuthorCreated").Advice.ShouldNotBeEmpty();
    [Fact] void should_not_advise_about_the_mechanical_name() => _cardsWithoutQuestions.Single(card => card.Key.Value == "Authors/AuthorCreated").Advice.ShouldBeEmpty();
    [Fact] void should_show_the_name_and_the_mechanical_name() => _cardsOfANameChange.Single(card => card.Key.Value == "Authors/AuthorCreated").Mechanical.Value.ShouldEqual("AuthorCreated");
    [Fact] void should_list_nothing_when_nothing_was_interpreted() => EventCard.From(null, []).ShouldBeEmpty();
}
#endif
