// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useEffect, useState } from 'react';
import { apiClient, toProblemDetails } from '../Api';
import type { ProblemDetails } from '../Api';
import type { SimulationStatus } from './SimulationStatus';

const pollIntervalInMilliseconds = 1000;

/**
 * Represents the polled simulation status.
 */
export interface PolledStatus {
    /** The most recent status, once one has arrived. */
    status?: SimulationStatus;

    /** The problem reading the status reported, if it did not succeed. */
    problem?: ProblemDetails;

    /** Takes the status a start or stop came back with, so polling picks up from it immediately. */
    apply: (status: SimulationStatus) => void;
}

/**
 * Reads the simulation status once on mount and then every second for as long as a run is in flight.
 * Polling stops on the first status that reports the run is no longer running.
 * @returns The {@link PolledStatus}.
 */
export const useSimulationStatus = (): PolledStatus => {
    const [status, setStatus] = useState<SimulationStatus | undefined>(undefined);
    const [problem, setProblem] = useState<ProblemDetails | undefined>(undefined);

    const refresh = useCallback(async () => {
        try {
            setStatus(await apiClient.get<SimulationStatus>('/api/simulation/status'));
            setProblem(undefined);
        } catch (error) {
            setProblem(toProblemDetails(error));
        }
    }, []);

    useEffect(() => {
        void refresh();
    }, [refresh]);

    const isRunning = status?.isRunning ?? false;

    useEffect(() => {
        if (!isRunning) {
            return;
        }

        const handle = window.setInterval(() => void refresh(), pollIntervalInMilliseconds);
        return () => window.clearInterval(handle);
    }, [isRunning, refresh]);

    return { status, problem, apply: setStatus };
};
