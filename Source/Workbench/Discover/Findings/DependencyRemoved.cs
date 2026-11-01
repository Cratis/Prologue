// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// A dependency that was found is no longer in the code.
/// </summary>
/// <param name="Subject">The system or folder that depended on the target.</param>
/// <param name="Target">What it depended on.</param>
/// <param name="Kind">How it depended on it.</param>
[EventType]
public record DependencyRemoved(AnalysisSubjectId Subject, DependencyTarget Target, DependencyKind Kind);
