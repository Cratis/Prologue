// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface CaptureLike {
    readonly capture: unknown;
    readonly occurred: Date;
    readonly http: { readonly method: string; readonly path: string; readonly status: number };
    readonly transactions: readonly { readonly tables: readonly { readonly table: string; readonly operation: number }[] }[];
}

export interface ParityLike {
    readonly observation: unknown;
    readonly occurred: Date;
    readonly method: string;
    readonly path: string;
    readonly shape: string;
    readonly legacyStatus: number;
    readonly shadowStatus: number;
    readonly outcome: number;
}

export interface HistoryEntry {
    readonly key: string;
    readonly at: Date;
    readonly source: 'legacy' | 'new';
    readonly what: string;
    readonly how: string;
}

/** The most entries one history shows. */
export const historyLimit = 50;

const operations = ['INSERT', 'UPDATE', 'DELETE'];
const outcomes = ['same decision', 'decided differently', 'new system unavailable', 'not mirrored'];

const mentions = (path: string, entity: string): boolean =>
    path.split('?')[0].split('/').some(segment => segment.toLowerCase() === entity.toLowerCase());

const written = (capture: CaptureLike): string => {
    const changes = capture.transactions.flatMap(transaction => transaction.tables.map(table => `${operations[table.operation] ?? 'CHANGE'} ${table.table}`));
    return [...new Set(changes)].join(', ');
};

/**
 * One history of an entity: what the legacy system was captured doing to it and what the new system decided when the same command
 * was mirrored, oldest first. It reads the latest captures and parity observations the page already holds, so it is bounded by them
 * and by `historyLimit`; the newest entries win when there are more.
 */
export const entityHistory = (captures: readonly CaptureLike[], parity: readonly ParityLike[], entity: string): readonly HistoryEntry[] => {
    const wanted = entity.trim();
    if (wanted === '') return [];

    const legacy: HistoryEntry[] = captures
        .filter(capture => mentions(capture.http.path, wanted))
        .map(capture => ({
            key: `legacy-${String(capture.capture)}`,
            at: capture.occurred,
            source: 'legacy',
            what: `${capture.http.method} ${capture.http.path} → ${capture.http.status}`,
            how: written(capture)
        }));

    const mirrored: HistoryEntry[] = parity
        .filter(observation => mentions(observation.path, wanted))
        .map(observation => ({
            key: `new-${String(observation.observation)}`,
            at: observation.occurred,
            source: 'new',
            what: observation.shadowStatus > 0 ? `Answered ${observation.shadowStatus} to ${observation.method} ${observation.path}` : `No answer to ${observation.method} ${observation.path}`,
            how: `shadow · ${outcomes[observation.outcome] ?? ''}`
        }));

    return [...legacy, ...mirrored]
        .sort((a, b) => a.at.getTime() - b.at.getTime() || (a.source === b.source ? 0 : a.source === 'legacy' ? -1 : 1))
        .slice(-historyLimit);
};
