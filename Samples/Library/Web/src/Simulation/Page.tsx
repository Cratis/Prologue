// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useRejection } from '../Api';
import { Card, Field, Page } from '../Shell';
import type { SimulationStatus } from './SimulationStatus';
import { useSimulationStatus } from './useSimulationStatus';

const defaultTransactionCount = '10000';

const describe = (status: SimulationStatus | undefined): string => {
    if (!status) {
        return 'Unknown';
    }

    if (status.isRunning) {
        return 'Running';
    }

    return status.completedAt === null ? 'Idle' : 'Completed';
};

/**
 * Puts the system under load. Start kicks off a run, and the status view polls once a second for as
 * long as the run is in flight.
 */
export const Simulation = () => {
    const { status, problem, apply } = useSimulationStatus();
    const rejection = useRejection();
    const [transactionCount, setTransactionCount] = useState(defaultTransactionCount);

    const start = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            apply(
                await apiClient.post<SimulationStatus>('/api/simulation/start', {
                    transactionCount: Number(transactionCount)
                })
            );
        });
    };

    const stop = () =>
        rejection.perform(async () => {
            apply(await apiClient.post<SimulationStatus>('/api/simulation/stop'));
        });

    const progressPercentage = Math.round((status?.progress ?? 0) * 100);

    return (
        <Page
            title='Simulation'
            subtitle='Put the system under load and watch how the transactions land.'
            problem={rejection.problem ?? problem}>
            <Card title='Run a simulation'>
                <form className='form-row' data-testid='simulation-form' onSubmit={start}>
                    <Field label='Transactions'>
                        <input
                            className='input'
                            type='number'
                            min='1'
                            data-testid='simulation-count'
                            value={transactionCount}
                            onChange={(event) => setTransactionCount(event.target.value)}
                        />
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='simulation-start'
                        disabled={rejection.isPerforming || (status?.isRunning ?? false)}>
                        Start
                    </button>
                    <button
                        className='button button-secondary'
                        type='button'
                        data-testid='simulation-stop'
                        disabled={rejection.isPerforming || !(status?.isRunning ?? false)}
                        onClick={() => void stop()}>
                        Stop
                    </button>
                </form>
            </Card>

            <Card title='Status'>
                <div className='stats'>
                    <div>
                        <span className='stat-label'>Status</span>
                        <span className='stat-value' data-testid='simulation-status'>
                            {describe(status)}
                        </span>
                    </div>
                    <div>
                        <span className='stat-label'>Succeeded</span>
                        <span className='stat-value numeric' data-testid='simulation-succeeded'>
                            {status?.succeeded ?? 0}
                        </span>
                    </div>
                    <div>
                        <span className='stat-label'>Rejected</span>
                        <span className='stat-value numeric' data-testid='simulation-rejected'>
                            {status?.rejected ?? 0}
                        </span>
                    </div>
                    <div>
                        <span className='stat-label'>Failed</span>
                        <span className='stat-value numeric' data-testid='simulation-failed'>
                            {status?.failed ?? 0}
                        </span>
                    </div>
                    <div>
                        <span className='stat-label'>Requested</span>
                        <span className='stat-value numeric'>{status?.requested ?? 0}</span>
                    </div>
                </div>

                <div className='progress'>
                    <div
                        className='progress-bar'
                        data-testid='simulation-progress'
                        style={{ width: `${progressPercentage}%` }}
                    />
                </div>
            </Card>
        </Page>
    );
};
