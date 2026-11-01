// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// How the last analysis of a system or folder ended.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Source">The commit or folder last read completely, when one was.</param>
/// <param name="Files">The number of files read by that analysis.</param>
/// <param name="Reason">Why the last attempt could not finish, when it could not.</param>
/// <param name="Occurred">When the outcome was recorded.</param>
public record AnalysisRun(AnalysisSubjectId Subject, SubjectName Name, AnalysisSource? Source, FileCount Files, FailureReason? Reason, DateTimeOffset Occurred);
