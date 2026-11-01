// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Log messages for <see cref="RepositoryCloner"/>.
/// </summary>
internal static partial class RepositoryClonerLogMessages
{
    [LoggerMessage(LogLevel.Warning, "Cloning {Repository} failed: {Reason}")]
    internal static partial void CloneFailed(this ILogger<RepositoryCloner> logger, string repository, string reason);
}
