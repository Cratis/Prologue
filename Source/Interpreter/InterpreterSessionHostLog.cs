// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Log messages for <see cref="InterpreterSessionHost"/>.
/// </summary>
internal static partial class InterpreterSessionHostLog
{
    [LoggerMessage(LogLevel.Error, "Proceeding or persisting the interpreter session failed")]
    internal static partial void WorkFailed(ILogger logger, Exception exception);
}
