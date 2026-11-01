// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { discoveredShapes } from '../discoveredShapes';

const capability = (...shapes: string[]) => ({ capability: 'x', shapes, file: 'f', runtime: 0, seen: 0 });
const map = {
    systems: [
        { subject: '', name: 'Lending', modules: [{ name: 'Loans', capabilities: [capability('POST /api/loans', 'POST /api/loans/{id}/return'), capability()] }] },
        { subject: '', name: 'Members', modules: [{ name: 'Members', capabilities: [capability('POST /api/loans'), capability('POST /api/members')] }] }
    ]
} as never;

describe('when listing the shapes of the application map', () => {
    it('lists the shapes of every capability of every system', () => expect(discoveredShapes(map)).toEqual(['POST /api/loans', 'POST /api/loans/{id}/return', 'POST /api/members']));
    it('lists each shape once', () => expect(discoveredShapes(map).filter(shape => shape === 'POST /api/loans')).toHaveLength(1));
    it('lists nothing before the map has answered', () => expect(discoveredShapes(undefined)).toEqual([]));
    it('lists nothing for a capability no command reaches', () => expect(discoveredShapes({ systems: [{ modules: [{ capabilities: [capability()] }] }] } as never)).toEqual([]));
});
