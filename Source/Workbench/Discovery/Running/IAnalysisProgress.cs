// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// Holds how far each analysis has come since the Receiver started, and tells observers when it changes.
/// </summary>
public interface IAnalysisProgress
{
    /// <summary>
    /// Observes the progress of every analysis, newest state first on subscription.
    /// </summary>
    /// <returns>The progress of each analysis, one per system or folder.</returns>
    IObservable<IReadOnlyList<AnalysisProgress>> Observe();

    /// <summary>
    /// Sets the progress of an analysis.
    /// </summary>
    /// <param name="progress">The progress.</param>
    void Set(AnalysisProgress progress);

    /// <summary>
    /// Gets the progress of an analysis.
    /// </summary>
    /// <param name="subject">The system or folder.</param>
    /// <returns>The progress, or null when it has not run since the Receiver started.</returns>
    AnalysisProgress? Get(AnalysisSubjectId subject);
}
