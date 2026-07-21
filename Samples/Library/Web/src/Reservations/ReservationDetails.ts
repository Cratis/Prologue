// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents a reservation as the API returns it.
 */
export interface ReservationDetails {
    /** The identifier of the reservation. */
    reservationId: number;

    /** The ISBN of the reserved title. */
    isbn: string;

    /** The title of the reserved book. */
    title: string;

    /** The identifier of the member who reserved it. */
    memberId: number;

    /** The name of the member who reserved it. */
    memberName: string;

    /** When the reservation was made. */
    reservedOn: string;
}
