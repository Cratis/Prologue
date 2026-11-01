// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Discovery.given;

namespace Cratis.Prologue.Workbench.Discovery.Recording.for_CodeAnalysisDecision;

public class when_an_analysis_could_not_finish : Specification
{
    List<object> _events;
    List<object> _again;

    void Because()
    {
        var before = new AnalysisFindings(
            [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return")],
            [Sample.Decision("Refuse a loan when no copy is available")],
            []);
        var current = Sample.Reduce(CodeAnalysisDecision.Decide(Sample.Outcome(before), null));

        // The agents were unreachable: only the routes were found.
        var partial = Sample.Outcome(new AnalysisFindings([Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"), Sample.Capability("Loans", "Renew loan", "POST /api/loans/{id}/renew")], [], []), failure: "The Prompter could not answer");
        _events = [.. CodeAnalysisDecision.Decide(partial, current)];
        _again = [.. CodeAnalysisDecision.Decide(partial, Sample.Reduce([.. CodeAnalysisDecision.Decide(Sample.Outcome(before), null), .. _events]))];
    }

    [Fact] void should_keep_what_it_did_find() => _events.OfType<CapabilityMapped>().Single().Capability.Value.ShouldEqual("Renew loan");
    [Fact] void should_remove_nothing_because_what_it_did_not_reach_is_not_gone() => _events.Exists(_ => _ is CapabilityRemoved or DecisionPointRemoved or DependencyRemoved).ShouldBeFalse();
    [Fact] void should_record_the_failure() => _events.OfType<CodeAnalysisFailed>().Single().Reason.Value.ShouldContain("Prompter");
    [Fact] void should_not_record_a_completion() => _events.OfType<CodeAnalysisCompleted>().ShouldBeEmpty();
    [Fact] void should_not_record_the_same_failure_twice() => _again.ShouldBeEmpty();
}
#endif
