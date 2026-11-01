// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Agents.for_AnalysisOutputParser.given;

namespace Cratis.Prologue.Workbench.Discover.Agents.for_AnalysisOutputParser;

public class when_parsing_decision_points : shown_files
{
    const string Answer = """
        {"decisionPoints":[
          {"rule":"Refuse a loan when no copy is available","file":"Loans/LoansController.cs","line":71,"command":"POST /api/loans","kind":"refusal"},
          {"rule":"Mark loans overdue after 28 days","file":"Jobs/OverdueSweep.cs","line":"18","kind":"schedule"},
          {"rule":"Line outside the file","file":"Loans/LoansController.cs","line":500,"kind":"refusal"},
          {"rule":"Line missing","file":"Loans/LoansController.cs","kind":"refusal"},
          {"rule":"Unknown kind","file":"Loans/LoansController.cs","line":5,"kind":"vibes"},
          {"rule":"Unseen file","file":"Other.cs","line":5,"kind":"refusal"},
          {"file":"Loans/LoansController.cs","line":5,"kind":"refusal"}
        ]}
        """;

    ParsedFindings<FoundDecisionPoint> _parsed;

    void Because() => _parsed = AnalysisOutputParser.DecisionPoints(Answer, _files);

    [Fact] void should_keep_the_valid_decision_points() => _parsed.Items.Count.ShouldEqual(2);
    [Fact] void should_keep_the_file_and_line() => (_parsed.Items[0].File.Value, _parsed.Items[0].Line.Value).ShouldEqual(("Loans/LoansController.cs", 71));
    [Fact] void should_normalize_the_command() => _parsed.Items[0].Shape.Value.ShouldEqual("POST /api/loans");
    [Fact] void should_read_a_line_written_as_text() => _parsed.Items[1].Line.Value.ShouldEqual(18);
    [Fact] void should_have_no_shape_when_the_code_does_not_say() => _parsed.Items[1].Shape.ShouldEqual(RouteShape.None);
    [Fact] void should_read_the_kind() => _parsed.Items[1].Kind.ShouldEqual(DecisionKind.Schedule);
    [Fact] void should_drop_a_line_outside_the_file() => _parsed.Dropped.Any(_ => _.Contains("outside", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_drop_a_missing_line() => _parsed.Dropped.Any(_ => _.Contains("Line missing", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_drop_an_unknown_kind() => _parsed.Dropped.Any(_ => _.Contains("vibes", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_drop_a_file_the_agent_was_not_shown() => _parsed.Dropped.Any(_ => _.Contains("Other.cs", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_drop_every_invalid_entry_with_a_reason() => _parsed.Dropped.Count.ShouldEqual(5);
}
#endif
