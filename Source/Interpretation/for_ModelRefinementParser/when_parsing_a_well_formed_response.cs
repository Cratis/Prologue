// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_ModelRefinementParser;

public class when_parsing_a_well_formed_response : Specification
{
    const string Response = """
        Here is the refinement you asked for:
        {
          "systemName": "LibrarySystem",
          "renames": { "CreateAuthor": "RegisterAuthor", "AuthorCreated": "AuthorRegistered" },
          "descriptions": { "module:Library": "Everything about the {library} domain" },
          "questions": []
        }
        Let me know if you need anything else.
        """;

    ModelRefinement _refinement;

    void Because() => _refinement = ModelRefinementParser.Parse(Response)!;

    [Fact] void should_parse_a_refinement() => _refinement.ShouldNotBeNull();
    [Fact] void should_parse_the_system_name() => _refinement.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_parse_every_rename() => _refinement.Renames.Count.ShouldEqual(2);
    [Fact] void should_parse_the_rename_targets() => _refinement.Renames["CreateAuthor"].ShouldEqual("RegisterAuthor");
    [Fact] void should_parse_descriptions_with_braces_in_their_text() => _refinement.Descriptions["module:Library"].ShouldEqual("Everything about the {library} domain");
    [Fact] void should_have_no_questions() => _refinement.Questions.ShouldBeEmpty();
}
#endif
