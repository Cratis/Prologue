// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// A decision point that was found is no longer in the code.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Rule">The rule it recorded.</param>
/// <param name="File">The file it was in.</param>
[EventType]
public record DecisionPointRemoved(AnalysisSubjectId Subject, RuleText Rule, SourcePath File);
