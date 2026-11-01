// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** Whether an address is an https address, the only kind a link to another site is made for. */
export const isHttpsUrl = (value: string): boolean => {
    try {
        return new URL(value).protocol === 'https:';
    } catch {
        return false;
    }
};
