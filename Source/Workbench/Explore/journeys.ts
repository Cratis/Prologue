// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SequenceLike } from './exploreTypes';

/** A journey as the panel shows it. */
export interface JourneyRow {
    readonly key: string;
    readonly share: number;
    readonly steps: readonly string[];

    /** The step the others went on to, when these things stopped short; shown as the unfinished ending. */
    readonly unfinishedBefore?: string;
    readonly note: string;
}

/** The most journeys the panel lists. */
export const journeysShown = 6;

const lastSegment = (shape: string): string => shape.split('/').filter(Boolean).pop() ?? shape;

/**
 * The ending drawn after the steps of an unfinished journey, such as (not return yet).
 * @param next The step the others went on to.
 */
export const endingLabel = (next: string): string => `(not ${lastSegment(next)} yet)`;

/**
 * The journeys to show: the most common sequences first, each with its share among things of its kind and, when others went
 * further, a note about where these stopped.
 * @param sequences The inferred sequences.
 * @param limit The most journeys to return.
 */
export const journeyRows = (sequences: readonly SequenceLike[], limit: number = journeysShown): JourneyRow[] =>
    [...sequences]
        .sort((a, b) => b.entities - a.entities || a.kind.localeCompare(b.kind))
        .slice(0, limit)
        .map(sequence => {
            const things = sequence.kind || 'things';
            const unfinished = sequence.continuedBy !== '';
            return {
                key: `${sequence.kind}:${sequence.steps.join('>')}`,
                share: sequence.share,
                steps: sequence.steps,
                unfinishedBefore: unfinished ? sequence.continuedBy : undefined,
                note: unfinished
                    ? `${sequence.entities} ${things} stopped here while others went on to ${lastSegment(sequence.continuedBy)}. Open, or never finished?`
                    : `${sequence.entities} ${things}.`
            };
        });
