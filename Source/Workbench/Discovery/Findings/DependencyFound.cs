// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// A dependency was found in the code, or how it is used changed.
/// </summary>
/// <param name="Subject">The system or folder that depends on the target.</param>
/// <param name="Target">What it depends on.</param>
/// <param name="Kind">How it depends on it.</param>
/// <param name="Detail">How it is used.</param>
[EventType]
public record DependencyFound(AnalysisSubjectId Subject, DependencyTarget Target, DependencyKind Kind, DependencyDetail Detail);
