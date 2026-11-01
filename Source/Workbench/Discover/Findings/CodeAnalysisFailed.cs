// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// An analysis could not finish, so what it found is kept and nothing is removed from the map.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Reason">Why it could not finish, with credentials redacted.</param>
[EventType]
public record CodeAnalysisFailed(AnalysisSubjectId Subject, SubjectName Name, FailureReason Reason);
