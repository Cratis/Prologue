// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { ChangeOperation } from '../../ChangeOperation';
import { lend, members, write } from '../for_exploreFixtures/exploreFixtures';
import { cellOperations, handlerOf, orderedShapes, percent, statusRows, tableColumns } from '../matrix';

describe('when building the matrix', () => {
    const shapes = [lend, members];
    const columns = tableColumns(shapes);
    const column = (label: string) => columns.find(candidate => candidate.label === label)!;

    it('adds a column for a table only transactions with no command write', () => expect(tableColumns(shapes, [write('AuditLog', [ChangeOperation.insert, 1])]).map(candidate => candidate.label)).toContain('AuditLog'));
    it('lists the most captured shape first', () => expect(orderedShapes(shapes).map(shape => shape.shape)).toEqual(['POST /api/members', 'POST /api/loans']));
    it('has a column for every table written', () => expect(columns.map(candidate => candidate.label).sort()).toEqual(['Inventory', 'Loans', 'Members', 'Reservations']));
    it('shows the share of runs a change happens in', () => expect(cellOperations(lend, column('Reservations'))[0].share).toBe(20));
    it('fades a change that only some of the writing runs make', () => expect(cellOperations(lend, column('Reservations'))[0].partial).toBe(true));
    it('does not fade a change every writing run makes, even when other runs were refused', () => expect(cellOperations(lend, column('Loans'))[0]).toMatchObject({ share: 80, partial: false, letter: 'I' }));
    it('describes the change in the tooltip', () => expect(cellOperations(lend, column('Reservations'))[0].title).toBe('delete Reservations in 20% of runs'));
    it('leaves a table the shape does not write empty', () => expect(cellOperations(members, column('Loans'))).toEqual([]));
    it('names the kind of change', () => expect(cellOperations(lend, column('Inventory'))[0].operation).toBe(ChangeOperation.update));
    it('lists how the shape was answered, the most common first', () => expect(statusRows(lend)).toEqual([
        { status: 201, captures: 8, share: 80, refused: false },
        { status: 409, captures: 2, share: 20, refused: true }
    ]));
    it('names the handler that served most captures', () => expect(handlerOf(lend)).toBe('LoansController.Lend'));
    it('has no handler without spans', () => expect(handlerOf(members)).toBeUndefined());
    it('never shows something that happened as zero percent', () => expect(percent(1, 5000)).toBe(1));
    it('shows nothing as zero percent', () => expect(percent(0, 10)).toBe(0));
});
