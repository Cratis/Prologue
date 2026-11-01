// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// Builds the dependencies of a prologue as the page shows them.
/// </summary>
public static class DependencyViews
{
    /// <summary>
    /// Names the dependencies in the findings.
    /// </summary>
    /// <param name="findings">What the analyses found.</param>
    /// <param name="definition">The prologue, which names the systems.</param>
    /// <returns>The dependencies.</returns>
    public static IEnumerable<CodeDependency> Build(CodeFindings findings, PrologueDefinition? definition) =>
        [.. findings.Dependencies.Select(_ => new CodeDependency(_.Subject, SystemNames.Of(_.Subject, definition, findings), _.Target, _.Kind, _.Detail))];
}
