// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface DifferenceLike {
    readonly observation: unknown;
    readonly occurred: Date;
    readonly shape: string;
    readonly path: string;
    readonly legacyStatus: number;
    readonly legacyDecision: number;
    readonly shadowStatus: number;
    readonly shadowDecision: number;
    readonly resolution: number;
}

export interface SliceShapes {
    readonly name: string;
    readonly shapes: readonly string[];
}

const open = 0;

/** What a system decided, in words; the index is the value of the generated `DecisionOutcome`. */
export const decisionLabel = (decision: number): string => ['No decision', 'Accepted', 'Refused'][decision] ?? 'No decision';

/** Whether a difference still blocks its slice. */
export const isOpen = (difference: Pick<DifferenceLike, 'resolution'>): boolean => difference.resolution === open;

/** The open differences of the slice that replaces a shape, newest first. */
export const openDifferencesOf = <T extends DifferenceLike>(slice: SliceShapes | undefined, differences: readonly T[]): readonly T[] =>
    slice ? differences.filter(difference => isOpen(difference) && slice.shapes.includes(difference.shape)) : [];

/** The slice to show differences for: the one picked if it still has any, otherwise the first slice that has open differences. */
export const sliceWithDifferences = (slices: readonly SliceShapes[], differences: readonly DifferenceLike[], picked: string): SliceShapes | undefined => {
    const wanted = slices.find(slice => slice.name === picked);
    if (wanted && openDifferencesOf(wanted, differences).length > 0) return wanted;
    return slices.find(slice => openDifferencesOf(slice, differences).length > 0);
};
