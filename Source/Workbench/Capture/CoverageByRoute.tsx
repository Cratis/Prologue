// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { EmptyState, Panel } from '../Components';
import { coverageRows } from './coverage';
import { CaptureData } from './useCaptureData';

/** How often each command shape was captured; shapes known from discovery that were never captured are called out. */
export const CoverageByRoute = ({ data, known }: { data: CaptureData; known: readonly string[] }) => {
    const { rows, neverCaptured } = coverageRows(data.shapes, known);
    return (
        <Panel title="Coverage by route">
            {rows.length === 0 && neverCaptured.length === 0
                ? <EmptyState title="Nothing captured yet" />
                : (
                    <div className="bars">
                        {rows.map(row => (
                            <div className="bar-row" key={row.shape}>
                                <span className="mono ellipsis" title={row.shape}>{row.shape}</span>
                                <span className="bar"><i style={{ width: `${row.share}%` }} /></span>
                                <span className="num">{row.captures}</span>
                            </div>
                        ))}
                        {neverCaptured.map(shape => (
                            <div className="bar-row" key={shape}>
                                <span className="mono ellipsis" title={shape}>{shape}</span>
                                <span className="bar" />
                                <span className="num warn-text">none</span>
                            </div>
                        ))}
                    </div>
                )}
            {neverCaptured.length > 0 && <p className="muted small-text note">Found in the code but never captured. Run a scenario that exercises them, or their events will be missing from the model.</p>}
        </Panel>
    );
};
