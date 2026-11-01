// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface ParityCounts {
    readonly matched: number;
    readonly mismatched: number;
    readonly unavailable: number;
    readonly skipped: number;
    readonly openDifferences: readonly unknown[];
}

export interface ParitySummary {
    /** The commands whose decisions were compared. */
    readonly compared: number;
    readonly matched: number;

    /** The share of compared decisions that match, 0 to 100. */
    readonly percent: number;
    readonly open: number;
    readonly unavailable: number;
    readonly skipped: number;
}

/** Parity of a slice: matching decisions out of those compared. A shadow that was unavailable and commands that were skipped are counted apart, never as differences. */
export const parityOf = (slice: ParityCounts): ParitySummary => {
    const compared = slice.matched + slice.mismatched;
    return {
        compared,
        matched: slice.matched,
        percent: compared === 0 ? 0 : (slice.matched / compared) * 100,
        open: slice.openDifferences.length,
        unavailable: slice.unavailable,
        skipped: slice.skipped
    };
};

/** The note under the parity bar about what was not compared, or nothing. */
export const parityNote = (summary: ParitySummary): string => {
    const notes = [
        summary.unavailable > 0 ? `${summary.unavailable} while the new system was unavailable` : '',
        summary.skipped > 0 ? `${summary.skipped} skipped` : ''
    ].filter(Boolean);
    return notes.join(', ');
};
