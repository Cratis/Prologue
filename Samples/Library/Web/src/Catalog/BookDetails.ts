// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents a catalog entry as the API returns it.
 */
export interface BookDetails {
    /** The identifier of the book. */
    bookId: number;

    /** The ISBN identifying the title. */
    isbn: string;

    /** The title of the book. */
    title: string;

    /** The identifier of the author who wrote it. */
    authorId: number;

    /** The name of the author who wrote it. */
    authorName: string;

    /** The free-text tags attached to the book. */
    tags: string[];
}
