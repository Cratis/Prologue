// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Database;

internal static partial class DatabaseInitializerLog
{
    [LoggerMessage(LogLevel.Information, "Library schema ready on {Provider} (created: {Created})")]
    internal static partial void SchemaReady(ILogger logger, string provider, bool created);
}
