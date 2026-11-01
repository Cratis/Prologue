// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface CoverageRow {
    readonly shape: string;
    readonly captures: number;
    readonly refused: number;
    /** The bar's width as a percentage of the most captured shape. */
    readonly share: number;
}

/**
 * Coverage by route: the shapes captured, the most captured first, followed by the shapes known from discovery that were never captured.
 * @param shapes The shapes captured.
 * @param known The shapes Discover found in the code, if it has run.
 */
export const coverageRows = (
    shapes: readonly { readonly shape: string; readonly captures: number; readonly refused: number }[],
    known: readonly string[] = []
): { readonly rows: CoverageRow[]; readonly neverCaptured: string[] } => {
    const most = Math.max(1, ...shapes.map(shape => shape.captures));
    const rows = [...shapes]
        .sort((a, b) => b.captures - a.captures || a.shape.localeCompare(b.shape))
        .map(shape => ({ ...shape, share: (shape.captures / most) * 100 }));
    const seen = new Set(shapes.map(shape => shape.shape));
    return { rows, neverCaptured: known.filter(shape => !seen.has(shape)) };
};
