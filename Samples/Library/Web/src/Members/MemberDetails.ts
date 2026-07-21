// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents a member as the API returns it.
 */
export interface MemberDetails {
    /** The identifier of the member. */
    memberId: number;

    /** The member's first name. */
    firstName: string;

    /** The member's last name. */
    lastName: string;
}
