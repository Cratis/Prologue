// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// A capability in the map.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Module">The module it belongs to.</param>
/// <param name="Capability">The capability.</param>
/// <param name="Shapes">The command shapes that reach it.</param>
/// <param name="File">The file it is implemented in.</param>
public record MappedCapability(AnalysisSubjectId Subject, ModuleName Module, CapabilityName Capability, IEnumerable<RouteShape> Shapes, SourcePath File);

/// <summary>
/// A decision point in the map.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Rule">The rule in a sentence.</param>
/// <param name="File">The file it is in.</param>
/// <param name="Line">The line it is on.</param>
/// <param name="Shape">The command shape it belongs to.</param>
/// <param name="Kind">What it does when it decides.</param>
public record MappedDecisionPoint(AnalysisSubjectId Subject, RuleText Rule, SourcePath File, LineNumber Line, RouteShape Shape, DecisionKind Kind);

/// <summary>
/// A dependency in the map.
/// </summary>
/// <param name="Subject">The system or folder that depends on the target.</param>
/// <param name="Target">What it depends on.</param>
/// <param name="Kind">How it depends on it.</param>
/// <param name="Detail">How it is used.</param>
public record MappedDependency(AnalysisSubjectId Subject, DependencyTarget Target, DependencyKind Kind, DependencyDetail Detail);
