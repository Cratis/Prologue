// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** A slice the Rebuild page already tracks. */
export interface TrackedSlice {
    readonly name: string;
    readonly shapes: readonly string[];
}

/** A slice of the model that can replace legacy commands. */
export interface ModelSliceLike {
    readonly name: string;
    readonly shapes: readonly string[];
    readonly isOpen: boolean;
}

/** A Rebuild slice to create. */
export interface PlannedSlice {
    readonly name: string;
    readonly shapes: readonly string[];
}

/** What accepting the model creates on the Rebuild page and what it leaves alone. */
export interface RebuildPlan {
    readonly create: readonly PlannedSlice[];
    readonly existing: readonly string[];
    readonly open: readonly string[];
}

const method = (shape: string): string => {
    const [verb, ...rest] = shape.trim().split(/\s+/);
    return rest.length === 0 ? shape.trim() : `${verb.toUpperCase()} ${rest.join(' ')}`;
};

/**
 * Plans the Rebuild slices for an accepted model. A slice is created when it has shapes it replaces and none of its suggestions is open;
 * one already tracked by name, or whose shapes are all replaced by another slice, is left alone, so running the plan twice creates nothing the second time.
 * @param slices The slices of the model.
 * @param tracked The slices the Rebuild page already tracks.
 */
export const planRebuildSlices = (slices: readonly ModelSliceLike[], tracked: readonly TrackedSlice[]): RebuildPlan => {
    const names = new Set(tracked.map(slice => slice.name.trim().toLowerCase()));
    const replaced = new Set(tracked.flatMap(slice => slice.shapes.map(method)));
    const create: PlannedSlice[] = [];
    const existing: string[] = [];
    const open: string[] = [];

    slices.filter(slice => slice.shapes.length > 0 && slice.name.trim().length > 0).forEach(slice => {
        const name = slice.name.trim();
        if (names.has(name.toLowerCase())) {
            existing.push(name);
            return;
        }

        if (slice.isOpen) {
            open.push(name);
            return;
        }

        const shapes = [...new Set(slice.shapes.map(method))].filter(shape => !replaced.has(shape));
        if (shapes.length === 0) {
            existing.push(name);
            return;
        }

        names.add(name.toLowerCase());
        shapes.forEach(shape => replaced.add(shape));
        create.push({ name, shapes });
    });

    return { create, existing, open };
};
