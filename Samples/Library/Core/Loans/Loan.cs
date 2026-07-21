// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;
using Library.Core.Members;

namespace Library.Core.Loans;

/// <summary>
/// Represents a copy of a title being out with a member.
/// </summary>
public class Loan
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the loan.
    /// </summary>
    public int LoanId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the lent book.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the lent book.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the member the book is lent to.
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// Gets or sets the member the book is lent to.
    /// </summary>
    public Member? Member { get; set; }

    /// <summary>
    /// Gets or sets when the book was lent out.
    /// </summary>
    public DateTimeOffset LentOn { get; set; }

    /// <summary>
    /// Gets or sets when the book was returned, or <see langword="null"/> while it is still out.
    /// </summary>
    public DateTimeOffset? ReturnedOn { get; set; }
}
