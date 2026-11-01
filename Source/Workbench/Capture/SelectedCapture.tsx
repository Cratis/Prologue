// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, Chip, DataTable, EmptyState, Panel } from '../Components';
import { DecisionOutcome } from '../Decisions/DecisionOutcome';
import { CaptureLike, ReasonLike } from './captureTypes';
import { explainDecision } from './explain';
import { operationLetter, operationName } from './format';
import { Timeline } from './Timeline';

export interface SelectedCaptureProps {
    readonly capture: CaptureLike | undefined;
    readonly reason: ReasonLike | undefined;
    readonly windowMilliseconds: number;
}

const Explanation = ({ capture, reason, windowMilliseconds }: Required<SelectedCaptureProps> & { capture: CaptureLike }) => (
    <ol className="explain">
        {explainDecision(capture, reason, windowMilliseconds).map(step => (
            <li key={step.text}><span>{step.text}<span className="evidence">{step.evidence}</span></span></li>
        ))}
    </ol>
);

/** One capture in detail: the decision with its explanation, the timeline lanes and the columns each transaction changed. */
export const SelectedCapture = ({ capture, reason, windowMilliseconds }: SelectedCaptureProps) => {
    const [explained, setExplained] = useState('');

    if (!capture) return <Panel title="Selected capture"><EmptyState title="Pick a capture from the feed" /></Panel>;

    const decided = capture.outcome !== DecisionOutcome.notDecided;
    const refused = capture.outcome === DecisionOutcome.refused;
    const changes = capture.transactions.flatMap(transaction => transaction.tables);
    return (
        <Panel title="Selected capture" aside={`capture ${capture.capture.slice(0, 8)}\u2026`}>
            {decided && (
                <div className="decision">
                    <Chip variant={refused ? 'bad' : 'ok'}>{refused ? 'Refused' : 'Accepted'}</Chip>
                    <b>{capture.http.shape}</b>
                    <span className="muted small-text">a decision the system made</span>
                    {explained !== capture.capture && <Button small className="push" onClick={() => setExplained(capture.capture)}>Explain this decision</Button>}
                </div>
            )}
            {explained === capture.capture && <Explanation capture={capture} reason={reason} windowMilliseconds={windowMilliseconds} />}
            <Timeline capture={capture} windowMilliseconds={windowMilliseconds} />
            {changes.length > 0
                ? (
                    <DataTable
                        caption="Columns changed per table"
                        rows={changes}
                        rowKey={change => `${change.schema}.${change.table}.${change.operation}`}
                        columns={[
                            { key: 'table', header: 'Table', render: change => change.table },
                            { key: 'op', header: 'Op', render: change => <span className={`op ${operationLetter(change.operation)}`} title={operationName(change.operation)}>{operationLetter(change.operation)}</span> },
                            { key: 'columns', header: 'Columns changed', render: change => <span className="mono">{change.columns.join(', ')}</span> }
                        ]} />
                )
                : <p className="muted note">{decided && refused ? `No database writes. A ${capture.http.status} response usually means a rule refused the command. That is evidence for a validation rule.` : 'No database writes.'}</p>}
        </Panel>
    );
};
