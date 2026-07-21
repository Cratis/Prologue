// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Formats a timestamp the API returned as a stable UTC string. Deliberately not locale-dependent, so
 * the two frontends render the same text for the same value.
 * @param value The timestamp as the API serialized it.
 * @returns The formatted timestamp, or an empty string when there is none.
 */
export const formatDateTime = (value: string | null | undefined): string => {
    if (!value) {
        return '';
    }

    const parsed = new Date(value);

    return Number.isNaN(parsed.getTime())
        ? ''
        : parsed.toISOString().replace('T', ' ').slice(0, 19);
};
