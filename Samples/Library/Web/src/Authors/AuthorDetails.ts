// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents an author as the API returns it.
 */
export interface AuthorDetails {
    /** The identifier of the author. */
    authorId: number;

    /** The author's first name. */
    firstName: string;

    /** The author's last name. */
    lastName: string;

    /** How many books in the catalog are attributed to the author. */
    bookCount: number;
}
