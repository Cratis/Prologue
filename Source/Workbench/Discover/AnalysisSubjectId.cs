// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// The identity of what was analyzed: a system of the prologue, or a folder that was pointed at.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnalysisSubjectId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="AnalysisSubjectId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator AnalysisSubjectId(Guid value) => new(value);

    /// <summary>
    /// Gets the subject for a system. A system keeps its identity, so its findings follow it.
    /// </summary>
    /// <param name="system">The system.</param>
    /// <returns>The subject.</returns>
    public static AnalysisSubjectId For(SystemId system) => new(system.Value);

    /// <summary>
    /// Gets the subject for a folder. The same folder is always the same subject, so analyzing it again updates its findings.
    /// </summary>
    /// <param name="folder">The folder path.</param>
    /// <returns>The subject.</returns>
    public static AnalysisSubjectId ForFolder(string folder) =>
        new(new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(folder.Trim().TrimEnd('/', '\\').ToLowerInvariant()))[..16]));
}
