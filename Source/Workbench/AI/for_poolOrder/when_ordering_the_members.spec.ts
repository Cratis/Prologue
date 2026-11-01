// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { effectiveOrder, moveMember } from '../poolOrder';

describe('when working out the order of the members', () => {
    it('keeps the chosen order for the members still in the pool', () => expect(effectiveOrder(['a', 'b', 'c'], ['c', 'a', 'b'])).toEqual(['c', 'a', 'b']));
    it('drops a chosen provider that left the pool', () => expect(effectiveOrder(['a', 'b'], ['x', 'b', 'a'])).toEqual(['b', 'a']));
    it('puts a member the order does not mention last', () => expect(effectiveOrder(['a', 'b', 'c'], ['b'])).toEqual(['b', 'a', 'c']));
    it('lists each member once', () => expect(effectiveOrder(['a', 'b'], ['b', 'b', 'a'])).toEqual(['b', 'a']));
});

describe('when moving a member', () => {
    it('moves it up', () => expect(moveMember(['a', 'b', 'c'], 'b', -1)).toEqual(['b', 'a', 'c']));
    it('moves it down', () => expect(moveMember(['a', 'b', 'c'], 'b', 1)).toEqual(['a', 'c', 'b']));
    it('leaves the first member where it is when moved up', () => expect(moveMember(['a', 'b'], 'a', -1)).toEqual(['a', 'b']));
    it('leaves the last member where it is when moved down', () => expect(moveMember(['a', 'b'], 'b', 1)).toEqual(['a', 'b']));
});
