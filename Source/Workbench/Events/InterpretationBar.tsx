// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, Panel } from '../Components';
import { InterpretationState } from './Interpreting/InterpretationState';
import { InterpretationStatus } from './Interpreting/InterpretationStatus';

export interface InterpretationBarProps {
    readonly status?: InterpretationStatus;
    readonly busy: boolean;
    readonly onRun: () => void;
}

/** Whether an interpretation is working, so the page can say so and not offer to start another. */
export const isInterpreting = (status?: InterpretationStatus): boolean =>
    status?.state === InterpretationState.queued || status?.state === InterpretationState.running;

const stateChip = (status: InterpretationStatus) => {
    switch (status.state) {
        case InterpretationState.completed: return <Chip variant={status.refined ? 'ok' : 'neutral'}>{status.refined ? 'Refined by the Interpreter agent' : 'Heuristics only'}</Chip>;
        case InterpretationState.awaitingAnswers: return <Chip variant="warn">Waiting for your answers</Chip>;
        case InterpretationState.failed: return <Chip variant="bad">Could not finish</Chip>;
        case InterpretationState.idle: return <Chip>Not run yet</Chip>;
        default: return <Chip variant="http">Working</Chip>;
    }
};

/** Runs the interpretation in the background and shows how it is going. */
export const InterpretationBar = ({ status, busy, onRun }: InterpretationBarProps) => {
    const working = isInterpreting(status);
    return (
        <Panel>
            <div className="run-bar">
                <div className={status?.state === InterpretationState.failed ? 'status failed' : 'status'} role="status">
                    {status && stateChip(status)}
                    <span>{status?.message || 'Interpret the captures into suggested events. Without an AI provider the interpreter names events from the tables and routes alone.'}</span>
                </div>
                <Button variant="primary" disabled={busy || working} onClick={onRun}>{working ? 'Interpreting…' : 'Run interpretation'}</Button>
            </div>
        </Panel>
    );
};
