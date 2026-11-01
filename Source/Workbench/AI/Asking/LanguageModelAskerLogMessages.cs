// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Log messages for <see cref="LanguageModelAsker"/>.
/// </summary>
internal static partial class LanguageModelAskerLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The {Agent} agent could not be asked: {Reason}")]
    internal static partial void CouldNotAsk(this ILogger<LanguageModelAsker> logger, string agent, string reason);

    [LoggerMessage(LogLevel.Error, "Asking the {Agent} agent failed")]
    internal static partial void AskingFailed(this ILogger<LanguageModelAsker> logger, string agent, Exception exception);
}
