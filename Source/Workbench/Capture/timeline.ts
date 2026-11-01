// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CaptureLike } from './captureTypes';

export type LaneKind = 'http' | 'db' | 'otel';

export interface LaneBlock {
    readonly kind: LaneKind;
    readonly label: string;
    readonly left: number;
    readonly width: number;
}

export interface Timeline {
    /** How many milliseconds the lanes show: the correlation window, widened if an observation lies beyond it. */
    readonly spanMilliseconds: number;
    readonly blocks: readonly LaneBlock[];
}

const minimumWidth = 12;
const minimumBlockMilliseconds = 40;

/**
 * Lays a capture's observations out in the HTTP, database and spans lanes. Offsets are relative to the first observation, and the
 * lanes span the prologue's correlation window, so a block near the right edge shows an observation that only just made it in.
 * @param capture The capture.
 * @param windowMilliseconds The prologue's correlation window.
 */
export const layoutTimeline = (capture: CaptureLike, windowMilliseconds: number): Timeline => {
    const ends = [
        capture.http.offsetMilliseconds + minimumBlockMilliseconds,
        ...capture.transactions.map(transaction => transaction.offsetMilliseconds + minimumBlockMilliseconds),
        ...capture.spans.map(span => span.offsetMilliseconds + Math.max(span.durationMilliseconds, minimumBlockMilliseconds))
    ];
    const spanMilliseconds = Math.max(windowMilliseconds, ...ends);
    const place = (offset: number, duration: number) => ({
        left: (offset / spanMilliseconds) * 100,
        width: Math.max(minimumWidth, (Math.max(duration, minimumBlockMilliseconds) / spanMilliseconds) * 100)
    });

    const lastEnd = Math.max(...ends);
    const hasCommand = capture.http.shape !== '';
    const blocks: LaneBlock[] = [
        ...(hasCommand ? [{ kind: 'http' as const, label: `${capture.http.method} ${capture.http.path}`, ...place(capture.http.offsetMilliseconds, lastEnd - capture.http.offsetMilliseconds) }] : []),
        ...capture.transactions.flatMap(transaction => transaction.tables.map(change => ({
            kind: 'db' as const,
            label: `${change.table}`,
            ...place(transaction.offsetMilliseconds, minimumBlockMilliseconds * 2)
        }))),
        ...capture.spans.map(span => ({ kind: 'otel' as const, label: span.name, ...place(span.offsetMilliseconds, span.durationMilliseconds) }))
    ];

    return { spanMilliseconds, blocks };
};
