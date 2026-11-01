// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { decisionLabel, openDifferencesOf, sliceWithDifferences } from '../differences';

const difference = (observation: string, shape: string, resolution = 0) => ({
    observation, occurred: new Date(), shape, path: '/api/loans', legacyStatus: 201, legacyDecision: 1, shadowStatus: 409, shadowDecision: 2, resolution
});

const slices = [{ name: 'LendBook', shapes: ['POST /api/loans'] }, { name: 'ReturnBook', shapes: ['POST /api/loans/{id}/return'] }];
const differences = [difference('a', 'POST /api/loans'), difference('b', 'POST /api/loans', 1), difference('c', 'POST /api/loans/{id}/return')];

describe('when choosing the differences to show', () => {
    it('names what each system decided', () => expect([0, 1, 2].map(decisionLabel)).toEqual(['No decision', 'Accepted', 'Refused']));
    it('shows only open differences of the slice', () => expect(openDifferencesOf(slices[0], differences).map(item => item.observation)).toEqual(['a']));
    it('shows nothing without a slice', () => expect(openDifferencesOf(undefined, differences)).toEqual([]));
    it('shows the slice that was picked when it has open differences', () => expect(sliceWithDifferences(slices, differences, 'ReturnBook')?.name).toBe('ReturnBook'));
    it('falls back to the first slice with open differences', () => expect(sliceWithDifferences(slices, differences, 'Nothing')?.name).toBe('LendBook'));
    it('falls back when the picked slice has none left', () => expect(sliceWithDifferences(slices, [difference('c', 'POST /api/loans/{id}/return')], 'LendBook')?.name).toBe('ReturnBook'));
    it('has no slice when nothing is open', () => expect(sliceWithDifferences(slices, [difference('a', 'POST /api/loans', 1)], '')).toBeUndefined());
});
