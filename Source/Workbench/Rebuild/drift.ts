// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChipVariant } from '../Components';

export interface DriftLike {
    readonly key: string;
    readonly kind: number;
    readonly shape: string;
    readonly column: string;
    readonly captures: number;
    readonly acknowledged: boolean;
}

export interface DriftRow {
    readonly key: string;
    readonly text: string;
    readonly captures: number;
    readonly status: string;
    readonly variant: ChipVariant;
    readonly canSuggest: boolean;
}

const newColumn = 1;

/** The rows of the drift table: a command shape the model does not cover, or a column a known shape newly writes, most captured first. */
export const driftRows = (items: readonly DriftLike[]): readonly DriftRow[] =>
    [...items]
        .sort((a, b) => b.captures - a.captures)
        .map(item => ({
            key: item.key,
            text: item.kind === newColumn ? `${item.shape} → also writes ${item.column}` : item.shape,
            captures: item.captures,
            status: item.acknowledged ? 'Events suggested' : item.kind === newColumn ? 'Changed · new column written' : 'No · new behavior',
            variant: item.acknowledged ? 'ok' : 'drift',
            canSuggest: !item.acknowledged
        }));
