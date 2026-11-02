// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The UTC day a pool's spend is counted on, in the form the backend records it.
 * @param now The moment to name the day of.
 * @returns The day, as yyyy-MM-dd.
 */
export const utcDay = (now: Date): string => now.toISOString().slice(0, 10);

/**
 * The tokens a pool used today. The recorded spend belongs to the last day the pool was used, so it counts for nothing on any other day.
 * @param spend What was recorded: the day and the tokens used on it, or undefined when the pool never used any.
 * @param today The UTC day, as yyyy-MM-dd.
 * @returns The tokens used today.
 */
export const usedToday = (spend: { readonly day: string; readonly tokens: number } | undefined, today: string): number =>
    spend && spend.day === today ? spend.tokens : 0;

/**
 * How much of a ceiling is used, for the bar.
 * @param used The tokens used today.
 * @param ceiling The ceiling; zero or less means the pool has none.
 * @returns A whole percentage from 0 to 100; 0 when there is no ceiling.
 */
export const usagePercent = (used: number, ceiling: number): number =>
    ceiling > 0 ? Math.min(100, Math.round(used / ceiling * 100)) : 0;
