// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents a loan as the API returns it.
 */
export interface LoanDetails {
    /** The identifier of the loan. */
    loanId: number;

    /** The ISBN of the lent title. */
    isbn: string;

    /** The title of the lent book. */
    title: string;

    /** The identifier of the borrowing member. */
    memberId: number;

    /** The name of the borrowing member. */
    memberName: string;

    /** When the book was lent out. */
    lentOn: string;

    /** When the book was returned, if it has been. */
    returnedOn: string | null;
}
