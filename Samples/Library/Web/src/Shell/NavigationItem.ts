// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents one entry in the header navigation.
 */
export interface NavigationItem {
    /** The route the entry navigates to. */
    path: string;

    /** The text shown in the navigation. */
    label: string;

    /** The test id the shared Playwright suite drives the entry by. */
    testId: string;
}
