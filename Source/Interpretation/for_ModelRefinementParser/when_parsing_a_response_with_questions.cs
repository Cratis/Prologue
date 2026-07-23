// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_ModelRefinementParser;

public class when_parsing_a_response_with_questions : Specification
{
    const string Response = """
        {
          "systemName": "",
          "renames": {},
          "descriptions": {},
          "questions": [
            {
              "prompt": "Is an author the same as a writer?",
              "context": "The evidence uses both terms",
              "choices": [
                { "label": "Yes", "description": "They are the same concept" },
                { "label": "No" }
              ]
            },
            { "prompt": "What should the system be called?" }
          ]
        }
        """;

    ModelRefinement _refinement;

    void Because() => _refinement = ModelRefinementParser.Parse(Response)!;

    [Fact] void should_parse_both_questions() => _refinement.Questions.Count.ShouldEqual(2);
    [Fact] void should_parse_the_prompt() => _refinement.Questions[0].Prompt.ShouldEqual("Is an author the same as a writer?");
    [Fact] void should_parse_the_context() => _refinement.Questions[0].Context.ShouldEqual("The evidence uses both terms");
    [Fact] void should_parse_the_choices() => _refinement.Questions[0].Choices.Count.ShouldEqual(2);
    [Fact] void should_parse_the_choice_description() => _refinement.Questions[0].Choices[0].Description.ShouldEqual("They are the same concept");
    [Fact] void should_default_a_missing_choice_description_to_empty() => _refinement.Questions[0].Choices[1].Description.ShouldEqual(string.Empty);
    [Fact] void should_default_missing_context_to_empty() => _refinement.Questions[1].Context.ShouldEqual(string.Empty);
    [Fact] void should_default_missing_choices_to_none() => _refinement.Questions[1].Choices.ShouldBeEmpty();
}
#endif
