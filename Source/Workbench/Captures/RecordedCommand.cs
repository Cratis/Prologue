// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The HTTP command of a capture, as it was observed.
/// </summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The path, without its query string.</param>
/// <param name="Status">The status the command was answered with.</param>
/// <param name="Trace">The trace the request carried; <see cref="TraceId.None"/> when it carried none.</param>
/// <param name="Shape">The shape of the command; <see cref="CommandShape.None"/> when the capture had no command.</param>
/// <param name="OffsetMilliseconds">When the command was observed, relative to the first observation of the capture.</param>
public record RecordedCommand(
    HttpMethodName Method,
    RequestPath Path,
    HttpStatus Status,
    TraceId Trace,
    CommandShape Shape,
    int OffsetMilliseconds)
{
    /// <summary>
    /// Gets what a capture without an HTTP command records: an empty command with no shape.
    /// </summary>
    public static readonly RecordedCommand None = new(HttpMethodName.None, RequestPath.None, HttpStatus.None, TraceId.None, CommandShape.None, 0);
}
