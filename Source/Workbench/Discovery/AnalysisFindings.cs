// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// A capability found in the code: something a module can do, with the command shapes that reach it.
/// </summary>
/// <param name="Module">The module the capability belongs to.</param>
/// <param name="Capability">The capability.</param>
/// <param name="Shapes">The command shapes that reach it; none for a capability that no HTTP command triggers.</param>
/// <param name="File">The file it is implemented in.</param>
public record FoundCapability(ModuleName Module, CapabilityName Capability, IEnumerable<RouteShape> Shapes, SourcePath File);

/// <summary>
/// A decision point found in the code: a place where it refuses, changes or schedules something.
/// </summary>
/// <param name="Rule">The rule in a sentence.</param>
/// <param name="File">The file it is in.</param>
/// <param name="Line">The line it is on.</param>
/// <param name="Shape">The command shape it belongs to; <see cref="RouteShape.None"/> when the code does not say.</param>
/// <param name="Kind">What it does when it decides.</param>
public record FoundDecisionPoint(RuleText Rule, SourcePath File, LineNumber Line, RouteShape Shape, DecisionKind Kind);

/// <summary>
/// A dependency found in the code.
/// </summary>
/// <param name="Target">What it depends on.</param>
/// <param name="Kind">How it depends on it.</param>
/// <param name="Detail">How it is used.</param>
public record FoundDependency(DependencyTarget Target, DependencyKind Kind, DependencyDetail Detail);

/// <summary>
/// Everything an analysis found in one system or folder.
/// </summary>
/// <param name="Capabilities">The capabilities.</param>
/// <param name="DecisionPoints">The decision points.</param>
/// <param name="Dependencies">The dependencies.</param>
public record AnalysisFindings(IEnumerable<FoundCapability> Capabilities, IEnumerable<FoundDecisionPoint> DecisionPoints, IEnumerable<FoundDependency> Dependencies)
{
    /// <summary>
    /// Gets findings with nothing in them.
    /// </summary>
    public static readonly AnalysisFindings Empty = new([], [], []);
}
