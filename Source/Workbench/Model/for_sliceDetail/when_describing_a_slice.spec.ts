// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { layoutBoard } from '../board';
import { sliceDetail } from '../sliceDetail';
import { library } from '../for_board/fixtures';

const matrix = [
    { shape: 'POST /api/loans', writes: [{ schema: 'dbo', table: 'Loans', operations: [{ operation: 0, runs: 3 }, { operation: 1, runs: 1 }], columns: [] }] },
    { shape: 'POST /api/authors', writes: [{ schema: 'dbo', table: 'Authors', operations: [{ operation: 0, runs: 2 }], columns: [] }] }
] as never;

describe('when describing a slice', () => {
    const slices = layoutBoard(library).slices;
    const lend = sliceDetail(slices[1], new Map([['POST /api/loans', 14]]), matrix);
    const returned = sliceDetail(slices[2], new Map(), matrix);

    it('shows the shapes it was observed as with their capture counts', () => expect(lend.observed).toEqual([{ shape: 'POST /api/loans', captures: 14 }]));
    it('counts a shape nothing was captured for as zero', () => expect(returned.observed).toEqual([{ shape: 'POST /api/loans/{id}/return', captures: 0 }]));
    it('lists the tables its shapes write with the kinds of change', () => expect(lend.writes).toEqual([{ table: 'dbo.Loans', operations: [0, 1] }]));
    it('leaves out the tables other shapes write', () => expect(lend.writes.map(write => write.table)).not.toContain('dbo.Authors'));
    it('lists the rules seen', () => expect(lend.rules).toEqual(['LendBook rule']));
    it('is ready for Screenplay when none of its events is open', () => expect(lend.readyForScreenplay).toBe(true));
    it('is not ready while an event is open', () => expect(returned.readyForScreenplay).toBe(false));
    it('names the open events', () => expect(returned.openEvents).toEqual(['BookReturned']));
});
