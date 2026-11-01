// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** Reads a list typed as comma or line separated values: trimmed, without blanks or repeats. */
export const parseList = (text: string): string[] => [...new Set(text.split(/[\n,]/).map(item => item.trim()).filter(item => item.length > 0))];

/** Writes a list the way `parseList` reads it, one value per comma-separated entry. */
export const formatList = (items: readonly string[]): string => items.join(', ');

/** Whether two lists hold the same values in the same order. */
export const sameList = (first: readonly string[], second: readonly string[]): boolean =>
    first.length === second.length && first.every((item, index) => item === second[index]);
