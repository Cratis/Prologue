// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// A decision point was found in the code, or where it is changed.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Rule">The rule in a sentence.</param>
/// <param name="File">The file it is in.</param>
/// <param name="Line">The line it is on.</param>
/// <param name="Shape">The command shape it belongs to; none when the code does not say.</param>
/// <param name="Kind">What it does when it decides.</param>
[EventType]
public record DecisionPointFound(AnalysisSubjectId Subject, RuleText Rule, SourcePath File, LineNumber Line, RouteShape Shape, DecisionKind Kind);
