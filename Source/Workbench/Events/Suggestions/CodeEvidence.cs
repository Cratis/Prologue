// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Decisions;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A place in the code Discover found that explains a suggestion.
/// </summary>
/// <param name="File">The source file.</param>
/// <param name="Line">The line in the file; none when Discover only knows the file.</param>
/// <param name="Rule">What the code decides there, in words.</param>
public record CodeEvidence(SourceFilePath File, LineNumber Line, string Rule);
