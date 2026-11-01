// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { hourSummary } from '../hours';

describe('when saying when transactions ran', () => {
    it('names the hour of a single hour', () => expect(hourSummary([{ hour: 2 }])).toBe('Every one ran during 02:00–03:00 UTC.'));
    it('names the range of neighboring hours', () => expect(hourSummary([{ hour: 3 }, { hour: 2 }])).toBe('Every one ran between 02:00 and 04:00 UTC.'));
    it('lists hours that are apart', () => expect(hourSummary([{ hour: 2 }, { hour: 14 }])).toBe('They ran at 02:00, 14:00 UTC.'));
    it('wraps the last hour of the day to midnight', () => expect(hourSummary([{ hour: 23 }])).toBe('Every one ran during 23:00–00:00 UTC.'));
    it('says nothing without hours', () => expect(hourSummary([])).toBe(''));
});
