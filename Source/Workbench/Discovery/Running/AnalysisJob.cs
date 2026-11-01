// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Analyzing;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// One analysis waiting to run.
/// </summary>
/// <param name="Prologue">The prologue it belongs to.</param>
/// <param name="Target">What to read.</param>
public record AnalysisJob(PrologueId Prologue, AnalysisTarget Target);
