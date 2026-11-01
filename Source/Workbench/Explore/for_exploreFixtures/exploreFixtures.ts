// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../../ChangeOperation';
import { ShapeLike, WriteLike } from '../exploreTypes';

export const write = (table: string, ...operations: [ChangeOperation, number][]): WriteLike => ({
    schema: 'dbo',
    table,
    operations: operations.map(([operation, runs]) => ({ operation, runs })),
    columns: []
});

/** Lending: 10 captures, 8 answered 201 writing Loans and Inventory (2 also delete a Reservation), 2 refused with 409 and nothing written. */
export const lend: ShapeLike = {
    shape: 'POST /api/loans',
    captures: 10,
    withoutWrites: 2,
    statuses: [
        { status: 409, captures: 2, withoutWrites: 2, writes: [] },
        { status: 201, captures: 8, withoutWrites: 0, writes: [write('Loans', [ChangeOperation.insert, 8])] }
    ],
    writes: [
        write('Loans', [ChangeOperation.insert, 8]),
        write('Inventory', [ChangeOperation.update, 8]),
        write('Reservations', [ChangeOperation.delete, 2])
    ],
    handlers: [{ span: 'Lending.Handle', captures: 1 }, { span: 'LoansController.Lend', captures: 9 }],
    scenarios: [{ scenario: 'Lend a copy', captures: 6 }],
    observations: []
};

export const members: ShapeLike = {
    shape: 'POST /api/members',
    captures: 30,
    withoutWrites: 0,
    statuses: [{ status: 201, captures: 30, withoutWrites: 0, writes: [] }],
    writes: [write('Members', [ChangeOperation.insert, 30])],
    handlers: [],
    scenarios: [],
    observations: []
};
