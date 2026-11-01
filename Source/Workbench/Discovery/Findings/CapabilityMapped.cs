// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// A capability was found in the code of a system or folder, or what is known of it changed.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Module">The module it belongs to.</param>
/// <param name="Capability">The capability.</param>
/// <param name="Shapes">The command shapes that reach it.</param>
/// <param name="File">The file it is implemented in.</param>
[EventType]
public record CapabilityMapped(AnalysisSubjectId Subject, ModuleName Module, CapabilityName Capability, IEnumerable<RouteShape> Shapes, SourcePath File);
