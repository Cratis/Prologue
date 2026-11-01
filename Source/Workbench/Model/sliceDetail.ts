// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../ChangeOperation';
import { ShapeLike } from '../Explore/exploreTypes';
import { BoardSlice } from './board';

/** A shape of a slice with how many captures it was seen in. */
export interface ObservedShape {
    readonly shape: string;
    readonly captures: number;
}

/** A table the slice's commands write, with the kinds of change seen. */
export interface WrittenTable {
    readonly table: string;
    readonly operations: readonly ChangeOperation[];
}

/** What the Model page shows about the selected slice. */
export interface SliceDetail {
    readonly observed: readonly ObservedShape[];
    readonly writes: readonly WrittenTable[];
    readonly rules: readonly string[];
    readonly openEvents: readonly string[];
    readonly readyForScreenplay: boolean;
}

const tableName = (schema: string, table: string): string => schema ? `${schema}.${table}` : table;

/**
 * Collects the evidence behind a slice: the shapes it was observed as with their capture counts, the tables those shapes write,
 * the rules seen, and whether any of its events is still open.
 * @param slice The board column.
 * @param counts How many captures each command shape was seen in.
 * @param matrix What captures did to each shape.
 */
export const sliceDetail = (slice: BoardSlice, counts: ReadonlyMap<string, number>, matrix: readonly ShapeLike[]): SliceDetail => {
    const shapes = new Set(slice.slice.shapes);
    const tables = new Map<string, Set<ChangeOperation>>();
    matrix.filter(profile => shapes.has(profile.shape)).flatMap(profile => profile.writes).forEach(write => {
        const name = tableName(write.schema, write.table);
        const known = tables.get(name) ?? new Set<ChangeOperation>();
        write.operations.forEach(entry => known.add(entry.operation));
        tables.set(name, known);
    });

    const openEvents = slice.events.filter(event => event.isOpen).map(event => event.name);
    return {
        observed: slice.slice.shapes.map(shape => ({ shape, captures: counts.get(shape) ?? 0 })),
        writes: [...tables.entries()].map(([table, operations]) => ({ table, operations: [...operations] })),
        rules: [...new Set(slice.slice.rules.map(rule => rule.text))],
        openEvents,
        readyForScreenplay: openEvents.length === 0
    };
};
