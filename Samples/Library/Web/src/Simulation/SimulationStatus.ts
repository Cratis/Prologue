// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents how a simulation run is going, as the API reports it.
 */
export interface SimulationStatus {
    /** Whether a run is in progress. */
    isRunning: boolean;

    /** How many transactions the run was asked to carry out. */
    requested: number;

    /** How many transactions completed with a success status. */
    succeeded: number;

    /** How many were rejected by a business rule — an expected outcome, not a failure. */
    rejected: number;

    /** How many failed outright. */
    failed: number;

    /** When the run started, if it has. */
    startedAt: string | null;

    /** When the run finished, if it has. */
    completedAt: string | null;

    /** The most recent failure message, if any. */
    lastError: string | null;

    /** How many transactions have been carried out, however they turned out. */
    completed: number;

    /** How far through the run is, as a fraction between zero and one. */
    progress: number;
}
