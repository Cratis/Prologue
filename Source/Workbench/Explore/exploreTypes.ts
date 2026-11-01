// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../ChangeOperation';

/** In how many runs one kind of change happened to a table. */
export interface OperationLike {
    readonly operation: ChangeOperation;
    readonly runs: number;
}

/** What captures did to one table. */
export interface WriteLike {
    readonly schema: string;
    readonly table: string;
    readonly operations: readonly OperationLike[];
    readonly columns: readonly string[];
}

/** The captures of a shape that were answered with one status. */
export interface StatusLike {
    readonly status: number;
    readonly captures: number;
    readonly withoutWrites: number;
    readonly writes: readonly WriteLike[];
}

/** Everything captured about one command shape. */
export interface ShapeLike {
    readonly shape: string;
    readonly captures: number;
    readonly withoutWrites: number;
    readonly statuses: readonly StatusLike[];
    readonly writes: readonly WriteLike[];
    readonly handlers: readonly { readonly span: string; readonly captures: number }[];
    readonly scenarios: readonly { readonly scenario: string; readonly captures: number }[];
    readonly observations: readonly { readonly text: string }[];
}

/** A table written by database transactions with no command. */
export interface UncorrelatedLike {
    readonly write: WriteLike;
    readonly transactions: number;
    readonly hours: readonly { readonly hour: number; readonly transactions: number }[];
}

/** A sequence of command shapes that things of one kind went through. */
export interface SequenceLike {
    readonly kind: string;
    readonly steps: readonly string[];
    readonly entities: number;
    readonly share: number;
    readonly continuedBy: string;
}
