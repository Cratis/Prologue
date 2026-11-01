// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_EvidenceText;

public class when_writing_the_evidence : a_library_interpretation
{
    string _text;
    string _nothing;

    void Because()
    {
        _text = EvidenceText.For(_facts);
        _nothing = EvidenceText.For(EvidenceFacts.None);
    }

    [Fact] void should_list_the_scenario_labels_of_a_shape() => _text.ShouldContain("Scenario labels of POST /api/loans/{id}: Lend a copy (4)");
    [Fact] void should_list_the_rules_found_in_the_code_with_file_and_line() => _text.ShouldContain("Member has 5 open loans (LoanPolicy.cs:22)");
    [Fact] void should_say_the_code_refuses() => _text.ShouldContain("Code refuses on POST /api/loans/{id}");
    [Fact] void should_be_empty_when_there_is_no_evidence() => _nothing.ShouldBeEmpty();
}
#endif
