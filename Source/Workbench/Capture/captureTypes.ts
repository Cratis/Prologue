// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../ChangeOperation';
import { DecisionOutcome } from '../Decisions/DecisionOutcome';
import { ReasonSource } from '../Decisions/ReasonSource';

// The parts of the generated proxies the pure modules in this folder read, so they can be built without the proxies.

export interface TableChangeLike {
    readonly schema: string;
    readonly table: string;
    readonly operation: ChangeOperation;
    readonly columns: readonly string[];
}

export interface TransactionLike {
    readonly tables: readonly TableChangeLike[];
    readonly offsetMilliseconds: number;
}

export interface SpanLike {
    readonly name: string;
    readonly service: string;
    readonly offsetMilliseconds: number;
    readonly durationMilliseconds: number;
}

export interface CaptureLike {
    readonly capture: string;
    readonly occurred: Date;
    readonly service: string;
    readonly http: {
        readonly method: string;
        readonly path: string;
        readonly status: number;
        readonly trace: string;
        readonly shape: string;
        readonly offsetMilliseconds: number;
    };
    readonly transactions: readonly TransactionLike[];
    readonly spanCount: number;
    readonly spans: readonly SpanLike[];
    readonly traceLinked: boolean;
    readonly scenario: string;
    readonly outcome: DecisionOutcome;
}

export interface ReasonLike {
    readonly source: ReasonSource;
    readonly text: string;
    readonly file: string;
    readonly line: number;
    readonly capability: string;
}

export interface BucketLike {
    readonly minute: Date;
    readonly captures: number;
}
