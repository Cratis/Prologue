// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Log messages for <see cref="InterpreterSession"/>.
/// </summary>
internal static partial class InterpreterSessionLog
{
    [LoggerMessage(LogLevel.Warning, "Language-model refinement failed; falling back to the unrefined event model")]
    internal static partial void RefinementFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Warning, "The language model's response held no parsable refinement; falling back to the unrefined event model")]
    internal static partial void UnparsableRefinement(ILogger logger);

    [LoggerMessage(LogLevel.Error, "The interpreter session failed")]
    internal static partial void SessionFailed(ILogger logger, Exception exception);
}
