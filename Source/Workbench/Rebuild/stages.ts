// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** The stages a slice moves through, in order; the index is the value of the generated `RebuildStage`. */
export const stageLabels = ['Observed', 'Modeled', 'Built', 'Shadowing', 'Cut over'] as const;

const shadowing = 3;
const cutOver = 4;

/** What the slice board offers to do next with a slice. */
export type SliceAction = 'model' | 'build' | 'shadow' | 'cutover';

export interface SliceProgress {
    readonly stage: number;
    readonly matched: number;
    readonly mismatched: number;
    readonly openDifferences: readonly unknown[];
}

export interface NextAction {
    readonly action: SliceAction;
    readonly label: string;
    readonly primary: boolean;

    /** Why the action cannot be taken yet, when it cannot. The server refuses it as well; this only says why before asking. */
    readonly blocked?: string;
}

const actions: Record<number, { action: SliceAction; label: string }> = {
    0: { action: 'model', label: 'Mark modeled' },
    1: { action: 'build', label: 'Mark built' },
    2: { action: 'shadow', label: 'Start shadowing' },
    3: { action: 'cutover', label: 'Cut over route' }
};

/**
 * The next step of a slice. Cutting over is offered while shadowing and is blocked until the slice has been compared at least once
 * and every difference is resolved; the Receiver enforces the same rule.
 */
export const nextAction = (slice: SliceProgress): NextAction | undefined => {
    const next = actions[slice.stage];
    if (!next) return undefined;
    if (slice.stage !== shadowing) return { ...next, primary: false };

    const open = slice.openDifferences.length;
    const blocked = open > 0
        ? `${open} ${open === 1 ? 'difference is' : 'differences are'} unresolved`
        : slice.matched + slice.mismatched === 0 ? 'Nothing has been compared yet' : undefined;
    return { ...next, primary: true, blocked };
};

/** Whether a slice can step back; only a slice that is still only observed cannot. */
export const canStepBack = (slice: Pick<SliceProgress, 'stage'>): boolean => slice.stage > 0;

export type PipelineState = 'on' | 'cur' | '';

/** The state of each stage in a slice's pipeline: the stages passed are on, the current one is marked, and a slice that is cut over has passed them all. */
export const pipeline = (stage: number): readonly { readonly label: string; readonly state: PipelineState }[] =>
    stageLabels.map((label, index) => ({ label, state: index < stage || stage === cutOver ? 'on' : index === stage ? 'cur' : '' }));

/** How many slices are cut over. */
export const cutOverCount = (slices: readonly Pick<SliceProgress, 'stage'>[]): number => slices.filter(slice => slice.stage === cutOver).length;
