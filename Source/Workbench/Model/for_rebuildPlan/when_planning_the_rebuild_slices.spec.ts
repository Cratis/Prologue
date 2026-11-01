// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { planRebuildSlices } from '../rebuildPlan';

const slices = [
    { name: 'LendBook', shapes: ['POST /api/loans'], isOpen: false },
    { name: 'ReturnBook', shapes: ['post /api/loans/{id}/return'], isOpen: false },
    { name: 'ReserveBook', shapes: ['POST /api/reservations'], isOpen: true },
    { name: 'ActiveLoans', shapes: [], isOpen: false },
    { name: 'LendAgain', shapes: ['POST /api/loans'], isOpen: false }
];

describe('when planning the rebuild slices', () => {
    const first = planRebuildSlices(slices, []);
    const created = first.create.map(item => ({ name: item.name, shapes: [...item.shapes] }));
    const again = planRebuildSlices(slices, created);
    const partly = planRebuildSlices(slices, [{ name: 'lendbook', shapes: ['POST /api/loans'] }]);

    it('creates a slice for each accepted slice that has shapes', () => expect(first.create.map(item => item.name)).toEqual(['LendBook', 'ReturnBook']));
    it('normalizes the method of a shape', () => expect(first.create[1].shapes).toEqual(['POST /api/loans/{id}/return']));
    it('leaves a slice with open suggestions out and says so', () => expect(first.open).toEqual(['ReserveBook']));
    it('creates nothing for a view without shapes', () => expect(first.create.map(item => item.name)).not.toContain('ActiveLoans'));
    it('does not let two slices replace the same shape', () => expect(first.existing).toEqual(['LendAgain']));
    it('creates nothing the second time', () => expect(again.create).toEqual([]));
    it('skips a slice that is tracked already whatever its case', () => expect(partly.create.map(item => item.name)).toEqual(['ReturnBook']));
    it('reports the slices it found tracked', () => expect(partly.existing).toEqual(['LendBook', 'LendAgain']));
});
