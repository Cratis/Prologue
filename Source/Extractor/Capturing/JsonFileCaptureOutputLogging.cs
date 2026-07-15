// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class JsonFileCaptureOutputLog
{
    [LoggerMessage(LogLevel.Information, "Rolling capture output to file {Path}")]
    internal static partial void RolledToFile(ILogger logger, string path);
}
