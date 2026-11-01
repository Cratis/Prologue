// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// A capability that was in the map is no longer in the code.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="Module">The module it belonged to.</param>
/// <param name="Capability">The capability.</param>
[EventType]
public record CapabilityRemoved(AnalysisSubjectId Subject, ModuleName Module, CapabilityName Capability);
