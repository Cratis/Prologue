// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Chip } from '../Components';
import { DecisionOutcome } from '../Decisions/DecisionOutcome';
import { CaptureLike } from './captureTypes';
import { clock, operationName } from './format';

export interface CaptureRowProps {
    readonly capture: CaptureLike;
    readonly selected: boolean;
    readonly onSelect: (id: string) => void;
}

/** One capture in the live feed: what was observed, as chips, with how it was correlated and what it was labeled. */
export const CaptureRow = ({ capture, selected, onSelect }: CaptureRowProps) => {
    const hasCommand = capture.http.shape !== '';
    const refused = capture.outcome === DecisionOutcome.refused;
    const classes = ['cap', selected ? 'sel' : '', capture.scenario ? 'scenario' : ''].filter(Boolean).join(' ');
    return (
        <div className={classes} role="button" tabIndex={0} aria-pressed={selected} onClick={() => onSelect(capture.capture)}
            onKeyDown={event => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); onSelect(capture.capture); } }}>
            <span className="t">{clock(capture.occurred)}</span>
            <div>
                <div className="row tight">
                    <b>{hasCommand ? capture.http.shape : 'Database transaction without a command'}</b>
                    {capture.scenario && <Chip variant="warn">{capture.scenario}</Chip>}
                    {refused && <Chip variant="bad">refused</Chip>}
                </div>
                <div className="obs">
                    {hasCommand && <Chip variant="http">{`${capture.http.method} ${capture.http.path} \u2192 ${capture.http.status}`}</Chip>}
                    {capture.transactions.flatMap((transaction, index) => transaction.tables.map(change => (
                        <Chip key={`${index}-${change.schema}.${change.table}-${change.operation}`} variant="db">{`${operationName(change.operation)} ${change.table}`}</Chip>
                    )))}
                    {capture.spanCount > 0 && <Chip variant="otel">{`${capture.spanCount} ${capture.spanCount === 1 ? 'span' : 'spans'}`}</Chip>}
                </div>
            </div>
            <span className={`corr${capture.traceLinked ? '' : ' muted'}`}>{!hasCommand ? 'no command' : capture.traceLinked ? 'trace-linked' : 'time window'}</span>
        </div>
    );
};
