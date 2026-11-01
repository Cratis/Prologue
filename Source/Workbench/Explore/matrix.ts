// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../ChangeOperation';
import { operationLetter, operationName } from '../Capture/format';
import { ShapeLike, StatusLike, WriteLike } from './exploreTypes';

/** A table the matrix has a column for. */
export interface TableColumn {
    readonly key: string;
    readonly label: string;
}

/** One kind of change in a cell of the matrix. */
export interface CellOperation {
    readonly operation: ChangeOperation;
    readonly letter: string;

    /** The share of the shape's runs the change happened in, as a whole percentage. */
    readonly share: number;

    /** Whether some of the runs that write skip this change, so the cell is drawn faded. */
    readonly partial: boolean;
    readonly title: string;
}

/** One response status of a shape. */
export interface StatusRow {
    readonly status: number;
    readonly captures: number;
    readonly share: number;
    readonly refused: boolean;
}

/**
 * A part of a whole as a whole percentage. Something that happened at all never reads as 0%.
 * @param part The part.
 * @param whole The whole.
 */
export const percent = (part: number, whole: number): number =>
    part <= 0 || whole <= 0 ? 0 : Math.min(100, Math.max(1, Math.round((part / whole) * 100)));

/** The key a table is known by in the matrix: its schema and name. */
export const tableKey = (write: WriteLike): string => `${write.schema}.${write.table}`;

/**
 * The columns of the matrix: every table any shape writes, or database transactions with no command write, the table written
 * by most writers first.
 * @param shapes The shapes.
 * @param outside What transactions with no command wrote.
 */
export const tableColumns = (shapes: readonly ShapeLike[], outside: readonly WriteLike[] = []): TableColumn[] => {
    const writers = new Map<string, { label: string; shapes: number }>();
    for (const write of [...shapes.flatMap(shape => shape.writes), ...outside]) {
        const known = writers.get(tableKey(write));
        writers.set(tableKey(write), { label: write.table, shapes: (known?.shapes ?? 0) + 1 });
    }

    return [...writers.entries()]
        .sort(([, a], [, b]) => b.shapes - a.shapes || a.label.localeCompare(b.label))
        .map(([key, { label }]) => ({ key, label }));
};

/**
 * The shapes in the order the matrix lists them: the most captured first.
 * @param shapes The shapes.
 */
export const orderedShapes = <T extends ShapeLike>(shapes: readonly T[]): T[] =>
    [...shapes].sort((a, b) => b.captures - a.captures || a.shape.localeCompare(b.shape));

/**
 * The kinds of change a shape made to a table. A change is faded when some of the runs that write skip it.
 * @param shape The shape.
 * @param column The table.
 */
export const cellOperations = (shape: ShapeLike, column: TableColumn): CellOperation[] => {
    const write = shape.writes.find(candidate => tableKey(candidate) === column.key);
    const writing = shape.captures - shape.withoutWrites;
    return (write?.operations ?? []).map(({ operation, runs }) => {
        const share = percent(runs, shape.captures);
        return { operation, letter: operationLetter(operation), share, partial: runs < writing, title: `${operationName(operation).toLowerCase()} ${write!.table} in ${share}% of runs` };
    });
};

/**
 * How a shape was answered, the most common status first.
 * @param shape The shape.
 */
export const statusRows = (shape: ShapeLike): StatusRow[] =>
    [...shape.statuses]
        .sort((a: StatusLike, b: StatusLike) => b.captures - a.captures)
        .map(({ status, captures }) => ({ status, captures, share: percent(captures, shape.captures), refused: status >= 400 }));

/**
 * The handler span of a shape: the span that began most of its captures.
 * @param shape The shape.
 * @returns The span, or undefined when no capture of the shape held a span.
 */
export const handlerOf = (shape: ShapeLike): string | undefined =>
    [...shape.handlers].sort((a, b) => b.captures - a.captures)[0]?.span;
