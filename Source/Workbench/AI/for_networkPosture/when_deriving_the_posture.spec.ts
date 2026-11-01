// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { derivePosture, PosturePool } from '../networkPosture';

const inside = new Set(['local', 'local-two']);
const isInside = (id: string) => inside.has(id);

const pool = (members: string[], inPriorityOrder = true, pausesBeforeFallback = false): PosturePool => ({ members, inPriorityOrder, pausesBeforeFallback });

describe('when deriving the posture', () => {
    it('is none without a pool that has a member', () => expect(derivePosture([pool([])], isInside)).toBe('none'));
    it('is local only when every member of every pool is inside', () => expect(derivePosture([pool(['local', 'local-two']), pool(['local'])], isInside)).toBe('localOnly'));
    it('is local first when a pool starts inside and pauses before going outside', () => expect(derivePosture([pool(['local', 'cloud'], true, true)], isInside)).toBe('localFirst'));
    it('leaves the network when a pool continues on the next provider outside', () => expect(derivePosture([pool(['local', 'cloud'], true, false)], isInside)).toBe('leavesNetwork'));
    it('leaves the network when an outside provider comes first', () => expect(derivePosture([pool(['cloud', 'local'], true, true)], isInside)).toBe('leavesNetwork'));
    it('does not promise to start inside when the pool picks by capacity', () => expect(derivePosture([pool(['local', 'cloud'], false, true)], isInside)).toBe('leavesNetwork'));
    it('takes the weakest posture across the pools', () => expect(derivePosture([pool(['local']), pool(['local', 'cloud'], true, true), pool(['cloud'])], isInside)).toBe('leavesNetwork'));
    it('ignores a pool without members', () => expect(derivePosture([pool([]), pool(['local'])], isInside)).toBe('localOnly'));
});
