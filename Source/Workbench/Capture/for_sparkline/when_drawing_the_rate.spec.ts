// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { capturesPerMinute, minuteSeries, sparklinePoints } from '../sparkline';

const at = (minute: number) => new Date(Date.UTC(2026, 10, 1, 12, minute));

describe('when working out the rate', () => {
    it('is zero without captures', () => expect(capturesPerMinute([])).toBe(0));
    it('averages over the minutes the buckets span, counting quiet minutes', () =>
        expect(capturesPerMinute([{ minute: at(0), captures: 6 }, { minute: at(3), captures: 6 }])).toBe(3));
    it('is not dragged down by buckets older than five minutes', () =>
        expect(capturesPerMinute([{ minute: at(-600), captures: 1 }, { minute: at(0), captures: 5 }])).toBe(5));
    it('keeps one decimal', () => expect(capturesPerMinute([{ minute: at(0), captures: 1 }, { minute: at(2), captures: 1 }])).toBe(0.7));
    it('counts a single bucket as one minute', () => expect(capturesPerMinute([{ minute: at(0), captures: 7 }])).toBe(7));
});

describe('when building the series', () => {
    it('fills quiet minutes with zero', () =>
        expect(minuteSeries([{ minute: at(0), captures: 2 }, { minute: at(3), captures: 5 }])).toEqual([2, 0, 0, 5]));
    it('is empty without buckets', () => expect(minuteSeries([])).toEqual([]));
    it('leaves out minutes older than thirty minutes before the newest', () =>
        expect(minuteSeries([{ minute: at(-600), captures: 9 }, { minute: at(0), captures: 2 }, { minute: at(1), captures: 3 }])).toEqual([2, 3]));
});

describe('when drawing the sparkline', () => {
    it('needs two values to draw a line', () => expect(sparklinePoints([4], 160, 30)).toBe(''));
    it('puts the highest value at the top and the lowest at the bottom', () => {
        const points = sparklinePoints([0, 10], 160, 30).split(' ').map(point => point.split(',').map(Number));
        expect(points).toEqual([[0, 27], [160, 3]]);
    });
});
