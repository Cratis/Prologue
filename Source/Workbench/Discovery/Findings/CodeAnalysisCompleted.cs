// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// An analysis read all of a system's or folder's code and every agent answered.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Source">The commit or folder that was read.</param>
/// <param name="Files">The number of files read.</param>
[EventType]
public record CodeAnalysisCompleted(AnalysisSubjectId Subject, SubjectName Name, AnalysisSource Source, FileCount Files);
