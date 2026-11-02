// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// A job Discover found in the code: something that runs on a schedule or in the background, outside any command.
/// </summary>
/// <param name="Name">What the job is called or does, in words.</param>
/// <param name="Detail">How it is used, such as the tables it writes; empty when the code does not say.</param>
/// <param name="File">The file it is in; none when Discover only knows the job by name.</param>
/// <param name="Line">The line it is on; none when not known.</param>
/// <param name="Module">The module of the capability implemented in the same file; none when no capability is.</param>
public record CodeJob(string Name, string Detail, SourcePath File, LineNumber Line, ModuleName Module);
