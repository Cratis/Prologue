// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Map;

/// <summary>
/// Works out what a system or analyzed folder is called.
/// </summary>
public static class SystemNames
{
    /// <summary>
    /// Gets the name of a subject: the system's current name, otherwise the name it had when it was analyzed.
    /// </summary>
    /// <param name="subject">The system or folder.</param>
    /// <param name="definition">The prologue.</param>
    /// <param name="findings">What was found.</param>
    /// <returns>The name.</returns>
    public static SubjectName Of(AnalysisSubjectId subject, PrologueDefinition? definition, CodeFindings findings)
    {
        var system = definition?.Systems.FirstOrDefault(_ => AnalysisSubjectId.For(_.Id) == subject);
        return system is not null ? new SubjectName(system.Name.Value) : findings.RunOf(subject)?.Name ?? new SubjectName("Folder");
    }
}
