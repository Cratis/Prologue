// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../../ChangeOperation';
import { DecisionOutcome } from '../../Decisions/DecisionOutcome';
import { CaptureLike } from '../captureTypes';

/** A refused loan that wrote nothing, with a span, for specs. */
export const refusedLoan: CaptureLike = {
    capture: 'c1',
    occurred: new Date('2026-11-01T12:00:00Z'),
    service: '',
    http: { method: 'POST', path: '/api/loans', status: 409, trace: 't1', shape: 'POST /api/loans', offsetMilliseconds: 0 },
    transactions: [],
    spanCount: 1,
    spans: [{ name: 'LoansController.Lend', service: 'Lending', offsetMilliseconds: 20, durationMilliseconds: 60 }],
    traceLinked: true,
    scenario: '',
    outcome: DecisionOutcome.refused
};

/** An accepted return that updated two tables, for specs. */
export const acceptedReturn: CaptureLike = {
    ...refusedLoan,
    capture: 'c2',
    http: { method: 'POST', path: '/api/loans/2649/return', status: 200, trace: 't2', shape: 'POST /api/loans/{id}/return', offsetMilliseconds: 0 },
    transactions: [{
        offsetMilliseconds: 120,
        tables: [
            { schema: 'public', table: 'Loans', operation: ChangeOperation.update, columns: ['ReturnedOn'] },
            { schema: 'public', table: 'Inventory', operation: ChangeOperation.update, columns: ['AvailableCount'] }
        ]
    }],
    outcome: DecisionOutcome.accepted
};
