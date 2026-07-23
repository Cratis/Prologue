// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Log messages for <see cref="LlmExtractionRefiner"/>.
/// </summary>
internal static partial class LlmExtractionRefinerLog
{
    [LoggerMessage(LogLevel.Warning, "Language-model name refinement failed; falling back to the unrefined event model")]
    internal static partial void RefinementFailed(ILogger logger, Exception exception);
}
