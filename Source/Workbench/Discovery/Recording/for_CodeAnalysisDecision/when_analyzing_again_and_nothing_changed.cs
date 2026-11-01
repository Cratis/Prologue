// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.given;

namespace Cratis.Prologue.Workbench.Discovery.Recording.for_CodeAnalysisDecision;

public class when_analyzing_again_and_nothing_changed : Specification
{
    List<object> _events;

    void Because()
    {
        var findings = new AnalysisFindings(
            [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return")],
            [Sample.Decision("Refuse a loan when no copy is available")],
            [Sample.Dependency("Notifications")]);
        var first = CodeAnalysisDecision.Decide(Sample.Outcome(findings), null);

        // The rule is worded a little differently and the shape is written in another case: still the same finding.
        var again = new AnalysisFindings(
            [Sample.Capability("loans", "Return Loan!", "post /API/loans/{id}/return")],
            [Sample.Decision("refuse a loan when no copy is available.")],
            [Sample.Dependency("Notifications")]);
        _events = [.. CodeAnalysisDecision.Decide(Sample.Outcome(again), Sample.Reduce(first))];
    }

    [Fact] void should_record_nothing() => _events.ShouldBeEmpty();
}
#endif
