// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { millisecondsToNextUtcDay, usagePercent, usedToday, utcDay } from '../poolUsage';

describe('when naming the day', () => {
    it('uses the UTC day, not the local one', () => expect(utcDay(new Date('2026-11-02T23:30:00-05:00'))).toBe('2026-11-03'));
});

describe('when waiting for the day to change', () => {
    it('counts down to the next UTC midnight', () => expect(millisecondsToNextUtcDay(new Date('2026-11-02T23:30:00Z'))).toBe(30 * 60 * 1000));
    it('waits a whole day at midnight itself', () => expect(millisecondsToNextUtcDay(new Date('2026-11-02T00:00:00Z'))).toBe(24 * 60 * 60 * 1000));
    it('crosses a month end', () => expect(millisecondsToNextUtcDay(new Date('2026-11-30T12:00:00Z'))).toBe(12 * 60 * 60 * 1000));
});

describe('when counting what a pool used today', () => {
    it('counts the recorded tokens on the day they were used', () => expect(usedToday({ day: '2026-11-02', tokens: 120 }, '2026-11-02')).toBe(120));
    it('counts nothing when the last use was another day', () => expect(usedToday({ day: '2026-11-01', tokens: 120 }, '2026-11-02')).toBe(0));
    it('counts nothing for a pool that never used any', () => expect(usedToday(undefined, '2026-11-02')).toBe(0));
});

describe('when showing how much of the ceiling is used', () => {
    it('rounds to a whole percentage', () => expect(usagePercent(333, 1000)).toBe(33));
    it('stops at a full bar when the ceiling is passed', () => expect(usagePercent(1500, 1000)).toBe(100));
    it('shows nothing without a ceiling', () => expect(usagePercent(500, 0)).toBe(0));
});
