// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Runtime;

namespace Cratis.Prologue.Workbench.Discovery.Map;

/// <summary>
/// A capability in the application map, with whether the running system was seen doing it.
/// </summary>
/// <param name="Capability">The capability.</param>
/// <param name="Shapes">The command shapes that reach it.</param>
/// <param name="File">The file it is implemented in.</param>
/// <param name="Runtime">Whether it was seen at runtime or is found only in code.</param>
/// <param name="Seen">How many captures belong to it.</param>
public record MapCapability(CapabilityName Capability, IEnumerable<RouteShape> Shapes, SourcePath File, RuntimeStatus Runtime, int Seen);

/// <summary>
/// A module in the application map.
/// </summary>
/// <param name="Name">The module.</param>
/// <param name="Capabilities">Its capabilities.</param>
public record MapModule(ModuleName Name, IEnumerable<MapCapability> Capabilities);

/// <summary>
/// A system or folder in the application map.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Modules">Its modules.</param>
public record MapSystem(AnalysisSubjectId Subject, SubjectName Name, IEnumerable<MapModule> Modules);
