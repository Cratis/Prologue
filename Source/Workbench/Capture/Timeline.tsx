// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CaptureLike } from './captureTypes';
import { layoutTimeline, LaneKind } from './timeline';

const lanes: readonly { readonly kind: LaneKind; readonly label: string }[] = [
    { kind: 'http', label: 'HTTP' },
    { kind: 'db', label: 'Database' },
    { kind: 'otel', label: 'Spans' }
];

/** The HTTP, database and span lanes of a capture inside the correlation window. */
export const Timeline = ({ capture, windowMilliseconds }: { capture: CaptureLike; windowMilliseconds: number }) => {
    const { spanMilliseconds, blocks } = layoutTimeline(capture, windowMilliseconds);
    return (
        <div className="lanes">
            {lanes.map(lane => (
                <div className="lane-row" key={lane.kind}>
                    <span className="lane-label">{lane.label}</span>
                    <div className="lane">
                        {blocks.filter(block => block.kind === lane.kind).map((block, index) => (
                            <span key={index} className={`blk ${block.kind}`} style={{ left: `${block.left}%`, width: `${Math.min(block.width, 100 - block.left)}%` }} title={block.label}>{block.label}</span>
                        ))}
                    </div>
                </div>
            ))}
            <div className="axis"><span>0 ms</span><span>{Math.round(spanMilliseconds / 2)} ms</span><span>{spanMilliseconds} ms{spanMilliseconds === windowMilliseconds ? ' window' : ''}</span></div>
            <div className="window-note">
                {capture.traceLinked
                    ? 'Spans joined by trace. Database rows joined by time.'
                    : 'No trace joined the spans. Everything is joined by time, so this grouping is weaker.'}
            </div>
        </div>
    );
};
