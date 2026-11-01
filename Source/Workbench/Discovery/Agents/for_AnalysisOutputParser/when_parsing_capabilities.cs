// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Agents.for_AnalysisOutputParser.given;

namespace Cratis.Prologue.Workbench.Discovery.Agents.for_AnalysisOutputParser;

public class when_parsing_capabilities : shown_files
{
    const string Plain = """
        {"capabilities":[
          {"module":"Loans","capability":"Return a copy","commands":["post /api/loans/2649/return","not a command"],"file":"Loans/LoansController.cs"},
          {"module":"Loans","capability":"Sweep overdue loans","commands":[],"file":"./Jobs/OverdueSweep.cs"}
        ]}
        """;

    const string Fenced = """
        Here is what I found:

        ```json
        {"capabilities":[{"module":"Loans","capability":"Lend a copy","commands":["POST /api/loans"],"file":"Loans/LoansController.cs"},],}
        ```
        Hope that helps.
        """;

    const string Mixed = """
        [{"module":"Loans","capability":"Lend a copy","commands":[],"file":"Loans/LoansController.cs"},
         {"module":"Loans","capability":"","file":"Loans/LoansController.cs"},
         {"module":"Loans","capability":"Ghost","file":"Nowhere.cs"},
         {"capability":"No module","file":"Loans/LoansController.cs"}]
        """;

    ParsedFindings<FoundCapability> _plain;
    ParsedFindings<FoundCapability> _fenced;
    ParsedFindings<FoundCapability> _mixed;
    ParsedFindings<FoundCapability> _prose;
    ParsedFindings<FoundCapability> _broken;
    ParsedFindings<FoundCapability> _wrongShape;

    void Because()
    {
        _plain = AnalysisOutputParser.Capabilities(Plain, _files);
        _fenced = AnalysisOutputParser.Capabilities(Fenced, _files);
        _mixed = AnalysisOutputParser.Capabilities(Mixed, _files);
        _prose = AnalysisOutputParser.Capabilities("I could not find anything.", _files);
        _broken = AnalysisOutputParser.Capabilities("{\"capabilities\":[{\"module\":", _files);
        _wrongShape = AnalysisOutputParser.Capabilities("{\"things\":[]}", _files);
    }

    [Fact] void should_read_valid_json() => _plain.Items.Count.ShouldEqual(2);
    [Fact] void should_normalize_the_commands_to_shapes() => _plain.Items[0].Shapes.Select(_ => _.Value).ShouldContainOnly("POST /api/loans/{id}/return");
    [Fact] void should_accept_a_file_written_with_a_dot_slash() => _plain.Items[1].File.Value.ShouldEqual("Jobs/OverdueSweep.cs");
    [Fact] void should_read_json_inside_a_code_fence_with_trailing_commas() => _fenced.Items.Single().Capability.Value.ShouldEqual("Lend a copy");
    [Fact] void should_accept_a_bare_array() => _mixed.Items.Single().Capability.Value.ShouldEqual("Lend a copy");
    [Fact] void should_drop_entries_that_are_not_usable() => _mixed.Dropped.Count.ShouldEqual(3);
    [Fact] void should_say_why_a_file_was_dropped() => _mixed.Dropped.Any(_ => _.Contains("Nowhere.cs", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_find_nothing_in_prose() => _prose.Items.ShouldBeEmpty();
    [Fact] void should_say_the_answer_holds_no_json() => _prose.Dropped.Single().ShouldContain("no JSON");
    [Fact] void should_find_nothing_in_cut_off_json() => _broken.Items.ShouldBeEmpty();
    [Fact] void should_say_the_answer_is_not_valid_json() => _broken.Dropped.Single().ShouldContain("JSON");
    [Fact] void should_say_when_the_list_is_missing() => _wrongShape.Dropped.Single().ShouldContain("capabilities");
}
#endif
