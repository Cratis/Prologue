// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { driftRows } from '../drift';

const item = (key: string, kind: number, shape: string, column: string, captures: number, acknowledged = false) => ({ key, kind, shape, column, captures, acknowledged });

describe('when listing drift', () => {
    const rows = driftRows([
        item('a', 0, 'PUT /api/members/{id}', '', 37),
        item('b', 1, 'POST /api/loans', 'dbo.Members.LoanCount', 121),
        item('c', 0, 'DELETE /api/reservations/{id}', '', 2, true)
    ]);

    it('lists the most captured first', () => expect(rows.map(row => row.key)).toEqual(['b', 'a', 'c']));
    it('shows a new column against the shape that writes it', () => expect(rows[0].text).toBe('POST /api/loans → also writes dbo.Members.LoanCount'));
    it('calls a column new behavior of a known command', () => expect(rows[0].status).toBe('Changed · new column written'));
    it('calls a new shape new behavior', () => expect(rows[1].status).toBe('No · new behavior'));
    it('offers events only for drift nobody has acted on', () => expect(rows.map(row => row.canSuggest)).toEqual([true, true, false]));
    it('marks suggested drift as handled', () => expect(rows[2]).toMatchObject({ status: 'Events suggested', variant: 'ok' }));
});
