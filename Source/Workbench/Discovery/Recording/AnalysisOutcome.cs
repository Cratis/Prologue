// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.Recording;

/// <summary>
/// What one analysis ended in.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Source">The commit or folder that was read.</param>
/// <param name="Files">The number of files read.</param>
/// <param name="Findings">What was found.</param>
/// <param name="Failure">Why the analysis could not finish, when it could not. Findings of an unfinished analysis are only ever added to the map: what it did not get to is not proof that the code lost it.</param>
public record AnalysisOutcome(AnalysisSubjectId Subject, SubjectName Name, AnalysisSource Source, FileCount Files, AnalysisFindings Findings, FailureReason? Failure);
