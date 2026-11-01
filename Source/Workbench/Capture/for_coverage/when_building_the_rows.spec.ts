// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { coverageRows } from '../coverage';

describe('when building coverage by route', () => {
    const result = coverageRows(
        [{ shape: 'POST /api/loans', captures: 10, refused: 2 }, { shape: 'POST /api/members', captures: 40, refused: 0 }],
        ['POST /api/loans', 'POST /api/inventory/{id}/lost']);

    it('puts the most captured first', () => expect(result.rows.map(row => row.shape)).toEqual(['POST /api/members', 'POST /api/loans']));
    it('scales the bars to the most captured', () => expect(result.rows.map(row => row.share)).toEqual([100, 25]));
    it('calls out the known shapes never captured', () => expect(result.neverCaptured).toEqual(['POST /api/inventory/{id}/lost']));
    it('has no call-outs without discovery', () => expect(coverageRows([], []).neverCaptured).toEqual([]));
});
