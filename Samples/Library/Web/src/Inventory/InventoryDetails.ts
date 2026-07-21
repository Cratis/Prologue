// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents an inventory record as the API returns it.
 */
export interface InventoryDetails {
    /** The ISBN of the title. */
    isbn: string;

    /** The title of the book. */
    title: string;

    /** The name of the author who wrote it. */
    authorName: string;

    /** How many copies the library owns. */
    totalCount: number;

    /** How many copies are available right now. */
    availableCount: number;
}
