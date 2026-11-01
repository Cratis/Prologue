// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Recording;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// Reads the code of one system or folder and works out what is in it.
/// </summary>
public interface IAnalysisRunner
{
    /// <summary>
    /// Runs an analysis, reporting progress as it goes.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>What it found, or why it could not finish.</returns>
    Task<AnalysisOutcome> Run(AnalysisJob job, CancellationToken cancellationToken);
}
