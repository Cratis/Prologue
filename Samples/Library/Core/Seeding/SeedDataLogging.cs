// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Seeding;

internal static partial class SeedDataLog
{
    [LoggerMessage(LogLevel.Information, "Seeded the library with {AuthorCount} authors, {MemberCount} members and {BookCount} books")]
    internal static partial void Seeded(ILogger logger, int authorCount, int memberCount, int bookCount);
}
