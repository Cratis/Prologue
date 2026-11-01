// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Panel } from '../Components';
import { endingLabel, journeyRows } from './journeys';
import { SequenceLike } from './exploreTypes';

/** The common sequences of commands that things went through, linked by the ids in request paths. */
export const JourneysPanel = ({ sequences }: { sequences: readonly SequenceLike[] }) => {
    const rows = journeyRows(sequences);
    return (
        <Panel title="Inferred journeys" aside={<>Linked by ids in request paths. Row values are never read.</>}>
            {rows.length === 0
                ? <p className="muted">No journeys yet. They need captures whose request path carries an id, such as /api/loans/2649/return.</p>
                : (
                    <div className="journeys">
                        {rows.map(row => (
                            <div className="journey" key={row.key}>
                                <span className="pct">{row.share}%</span>
                                <div>
                                    <div className="path">
                                        {row.steps.map((step, index) => (
                                            <span key={`${index}:${step}`} className="path-item">
                                                {index > 0 && <span className="arrow">→</span>}
                                                <span className="step mono">{step}</span>
                                            </span>
                                        ))}
                                        {row.unfinishedBefore && <span className="path-item"><span className="arrow">→</span><span className="step end">{endingLabel(row.unfinishedBefore)}</span></span>}
                                    </div>
                                    <div className="muted note">{row.note}</div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
        </Panel>
    );
};
