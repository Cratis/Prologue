// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Discovery.Recording;
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.given;

/// <summary>
/// Findings and outcomes the specs share.
/// </summary>
public static class Sample
{
    public static readonly AnalysisSubjectId Subject = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    public static FoundCapability Capability(string module, string capability, params string[] shapes) =>
        new(new ModuleName(module), new CapabilityName(capability), [.. shapes.Select(_ => new RouteShape(_))], new SourcePath($"{module}/{module}Controller.cs"));

    public static FoundDecisionPoint Decision(string rule, int line = 71, string shape = "POST /api/loans", DecisionKind kind = DecisionKind.Refusal) =>
        new(new RuleText(rule), new SourcePath("Loans/LoansController.cs"), new LineNumber(line), new RouteShape(shape), kind);

    public static FoundDependency Dependency(string target, DependencyKind kind = DependencyKind.Http, string detail = "POST /notify") =>
        new(new DependencyTarget(target), kind, new DependencyDetail(detail));

    public static AnalysisOutcome Outcome(AnalysisFindings findings, string source = "a41c9e2", int files = 12, string? failure = null) =>
        new(Subject, new SubjectName("Lending"), new AnalysisSource(source), new FileCount(files), findings, failure is null ? null : new FailureReason(failure));

    public static CodeFindings Reduce(IEnumerable<object> events)
    {
        var id = new Prologues.PrologueId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var context = EventContext.EmptyWithEventSourceId(id);
        var state = CodeFindings.Empty(id);
        foreach (var @event in events)
        {
            state = @event switch
            {
                CapabilityMapped mapped => CodeFindings.Reduce(mapped, state, context),
                CapabilityRemoved removed => CodeFindings.Reduce(removed, state)!,
                DecisionPointFound found => CodeFindings.Reduce(found, state, context),
                DecisionPointRemoved removed => CodeFindings.Reduce(removed, state)!,
                DependencyFound found => CodeFindings.Reduce(found, state, context),
                DependencyRemoved removed => CodeFindings.Reduce(removed, state)!,
                CodeAnalysisCompleted completed => CodeFindings.Reduce(completed, state, context),
                CodeAnalysisFailed failed => CodeFindings.Reduce(failed, state, context),
                _ => state
            };
        }

        return state;
    }
}
#endif
