// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Authors;

/// <summary>
/// Represents the details needed to register an author.
/// </summary>
/// <param name="FirstName">The author's first name.</param>
/// <param name="LastName">The author's last name.</param>
public record RegisterAuthor(string FirstName, string LastName);

/// <summary>
/// Represents an author as returned to callers.
/// </summary>
/// <param name="AuthorId">The identifier of the author.</param>
/// <param name="FirstName">The author's first name.</param>
/// <param name="LastName">The author's last name.</param>
/// <param name="BookCount">How many books in the catalog are attributed to the author.</param>
public record AuthorDetails(int AuthorId, string FirstName, string LastName, int BookCount);
