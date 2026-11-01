// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Link } from '../Shell/Router';
import { Chip, DataColumn, DataTable, Panel } from '../Components';
import { CodeDecisionPoint } from './Map/CodeDecisionPoint';
import { DecisionRuntime } from './Runtime/DecisionRuntime';
import { describeHappened, runtimeChip } from './discoverState';

export interface DecisionPointsPanelProps {
    readonly decisionPoints: readonly CodeDecisionPoint[];
    readonly readOnly?: boolean;
}

/** Where the code decides, with how often it happened at runtime; a decision point never exercised is flagged as code only. */
export const DecisionPointsPanel = ({ decisionPoints, readOnly = false }: DecisionPointsPanelProps) => {
    const columns: DataColumn<CodeDecisionPoint>[] = [
        { key: 'where', header: 'Where', render: point => <span className="mono codeev">{`${point.file}:${point.line}`}</span> },
        { key: 'decision', header: 'Decision', render: point => point.rule },
        { key: 'runtime', header: 'At runtime', render: point => <span className="num">{describeHappened(point)}</span> },
        {
            key: 'status', header: '', render: point => {
                const chip = runtimeChip(point.runtime);
                return (
                    <span className="row">
                        <Chip variant={chip.variant}>{chip.label}</Chip>
                        {!readOnly && point.runtime === DecisionRuntime.codeOnly && <Link to="/capture" className="btn small ghost">Run a scenario</Link>}
                    </span>
                );
            }
        }
    ];

    return (
        <Panel title="Decision points" aside="Where the code decides, matched to what the system did">
            <DataTable
                caption="Decision points found in the code"
                columns={columns}
                rows={decisionPoints}
                rowKey={point => `${String(point.subject)}|${point.file}|${point.rule}`}
                emptyTitle="No decision points found yet" />
        </Panel>
    );
};
