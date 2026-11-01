// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Chip, Panel } from '../Components';
import { BoundaryChoice } from './Boundaries/BoundaryChoice';
import { ConsistencyBoundary } from './Boundaries/ConsistencyBoundary';

export interface BoundariesPanelProps {
    readonly boundaries: readonly ConsistencyBoundary[];
    readonly choices: readonly BoundaryChoice[];
    readonly busy: (boundary: string) => boolean;
    readonly onChoose: (boundary: string, option: string) => void;
}

/** The consistency boundaries the evidence raises, with the option the interpreter suggests first. A choice becomes a rule in the accepted model. */
export const BoundariesPanel = ({ boundaries, choices, busy, onChoose }: BoundariesPanelProps) => (
    <Panel title="Consistency boundaries" aside="Where each rule must hold at once">
        <div className="grid">
            {boundaries.map(boundary => {
                const chosen = choices.find(choice => choice.key === boundary.key)?.option;
                return (
                    <div key={boundary.key} className={chosen ? 'q answered' : 'q'}>
                        <div><b>{boundary.rule}</b></div>
                        <div className="muted small-print">{boundary.evidence}</div>
                        <div className="opts">
                            {boundary.options.map((option, index) => (
                                <button key={option.key} type="button" disabled={busy(`boundary-${boundary.key}`)} aria-pressed={chosen === option.key} onClick={() => onChoose(boundary.key, option.key)}>
                                    {option.label}{index === 0 && <> <Chip variant="ok">suggested</Chip></>}
                                </button>
                            ))}
                        </div>
                    </div>
                );
            })}
            {boundaries.length === 0 && <p className="muted small-print">No rule has to hold across several things at once in what was captured.</p>}
        </div>
    </Panel>
);
