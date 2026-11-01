// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Extraction;

/// <summary>
/// A source file found under the folder being analyzed.
/// </summary>
/// <param name="Path">The path relative to the folder, with forward slashes.</param>
/// <param name="Size">The size in bytes.</param>
public record SourceFile(SourcePath Path, long Size);
