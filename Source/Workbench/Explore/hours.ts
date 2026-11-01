// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const clock = (hour: number): string => `${String(hour).padStart(2, '0')}:00`;

/**
 * Says when during the day transactions ran, in UTC.
 * @param hours The hours that had transactions.
 */
export const hourSummary = (hours: readonly { readonly hour: number }[]): string => {
    const sorted = [...hours].map(entry => entry.hour).sort((a, b) => a - b);
    if (sorted.length === 0) return '';
    if (sorted.length === 1) return `Every one ran during ${clock(sorted[0])}–${clock((sorted[0] + 1) % 24)} UTC.`;
    const contiguous = sorted[sorted.length - 1] - sorted[0] === sorted.length - 1;
    return contiguous
        ? `Every one ran between ${clock(sorted[0])} and ${clock((sorted[sorted.length - 1] + 1) % 24)} UTC.`
        : `They ran at ${sorted.map(clock).join(', ')} UTC.`;
};
