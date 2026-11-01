// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { parityNote, parityOf } from '../parity';

const counts = (matched: number, mismatched: number, unavailable = 0, skipped = 0, open = mismatched) =>
    ({ matched, mismatched, unavailable, skipped, openDifferences: Array.from({ length: open }, () => 'difference') });

describe('when summarizing parity', () => {
    it('compares only the decisions that were made', () => expect(parityOf(counts(899, 3, 40, 2)).compared).toBe(902));
    it('does not count an unavailable shadow as a difference', () => expect(parityOf(counts(10, 0, 5)).percent).toBe(100));
    it('measures the share that match', () => expect(parityOf(counts(3, 1)).percent).toBe(75));
    it('is zero before anything is compared', () => expect(parityOf(counts(0, 0, 4)).percent).toBe(0));
    it('counts the open differences', () => expect(parityOf(counts(5, 2, 0, 0, 1)).open).toBe(1));
    it('notes what was not compared', () => expect(parityNote(parityOf(counts(5, 0, 4, 2)))).toBe('4 while the new system was unavailable, 2 skipped'));
    it('has no note when everything was compared', () => expect(parityNote(parityOf(counts(5, 0)))).toBe(''));
});
