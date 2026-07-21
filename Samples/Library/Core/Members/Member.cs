// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Members;

/// <summary>
/// Represents someone registered with the library, who can reserve and borrow books.
/// </summary>
public class Member
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the member.
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// Gets or sets the member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
