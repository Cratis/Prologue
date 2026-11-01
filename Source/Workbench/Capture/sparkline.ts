// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { BucketLike } from './captureTypes';

const recentMinutes = 5;
const seriesMinutes = 30;

const within = (buckets: readonly BucketLike[], minutes: number): BucketLike[] => {
    if (buckets.length === 0) return [];
    const newest = buckets[buckets.length - 1].minute.getTime();
    return buckets.filter(bucket => newest - bucket.minute.getTime() < minutes * 60_000);
};

/**
 * The captures per minute over the last few minutes with captures: the sum of the buckets in the five minutes up to the newest bucket
 * divided by the minutes they span (a quiet minute in between counts), to one decimal. Older buckets do not drag the rate down.
 */
export const capturesPerMinute = (buckets: readonly BucketLike[]): number => {
    const recent = within(buckets, recentMinutes);
    if (recent.length === 0) return 0;
    const minutes = Math.max(1, Math.round((recent[recent.length - 1].minute.getTime() - recent[0].minute.getTime()) / 60_000) + 1);
    return Math.round((recent.reduce((sum, bucket) => sum + bucket.captures, 0) / minutes) * 10) / 10;
};

/** One value per minute from the first to the last of the last thirty minutes with captures, with 0 for the minutes that had none. */
export const minuteSeries = (buckets: readonly BucketLike[]): number[] => {
    const recent = within(buckets, seriesMinutes);
    if (recent.length === 0) return [];
    const first = recent[0].minute.getTime();
    const counts = new Map(recent.map(bucket => [Math.round((bucket.minute.getTime() - first) / 60_000), bucket.captures]));
    const minutes = Math.max(...counts.keys()) + 1;
    return Array.from({ length: minutes }, (_, index) => counts.get(index) ?? 0);
};

/** The points of a polyline drawing the series in a box of the given size, or an empty string when there are fewer than two values. */
export const sparklinePoints = (series: readonly number[], width: number, height: number, padding = 3): string => {
    if (series.length < 2) return '';
    const max = Math.max(...series, 1);
    return series
        .map((value, index) => `${(index / (series.length - 1)) * width},${height - padding - (value / max) * (height - padding * 2)}`)
        .join(' ');
};
