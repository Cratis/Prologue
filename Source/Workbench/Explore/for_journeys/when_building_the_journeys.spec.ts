// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { endingLabel, journeyRows } from '../journeys';

describe('when building the journeys', () => {
    const rows = journeyRows([
        { kind: 'loans', steps: ['POST /api/loans/{id}/lend'], entities: 75, share: 14, continuedBy: 'POST /api/loans/{id}/return' },
        { kind: 'loans', steps: ['POST /api/loans/{id}/lend', 'POST /api/loans/{id}/return'], entities: 312, share: 58, continuedBy: '' },
        { kind: 'books', steps: ['POST /api/books/{id}/tags'], entities: 3, share: 100, continuedBy: '' }
    ]);

    it('lists the most common sequence first', () => expect(rows.map(row => row.share)).toEqual([58, 14, 100]));
    it('keeps the steps in order', () => expect(rows[0].steps).toEqual(['POST /api/loans/{id}/lend', 'POST /api/loans/{id}/return']));
    it('does not call a finished sequence unfinished', () => expect(rows[0].unfinishedBefore).toBeUndefined());
    it('notes how many things stopped and where the others went', () => expect(rows[1].note).toBe('75 loans stopped here while others went on to return. Open, or never finished?'));
    it('keeps the step the others went on to', () => expect(rows[1].unfinishedBefore).toBe('POST /api/loans/{id}/return'));
    it('counts the things of a finished sequence', () => expect(rows[0].note).toBe('312 loans.'));
    it('labels the ending of an unfinished journey', () => expect(endingLabel('POST /api/loans/{id}/return')).toBe('(not return yet)'));
    it('shows at most the limit', () => expect(journeyRows([], 3)).toEqual([]));
    it('limits the number of journeys', () => expect(journeyRows(Array.from({ length: 9 }, (_, index) => ({ kind: 'a', steps: [`step${index}`], entities: index, share: 1, continuedBy: '' })), 4)).toHaveLength(4));
});
