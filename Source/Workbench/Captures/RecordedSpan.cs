// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// A telemetry span of a capture. Its attributes are not kept.
/// </summary>
/// <param name="Name">The name of the span.</param>
/// <param name="Service">The service the span was reported by.</param>
/// <param name="OffsetMilliseconds">When the span started, relative to the first observation of the capture.</param>
/// <param name="DurationMilliseconds">How long the span lasted.</param>
public record RecordedSpan(SpanName Name, ServiceName Service, int OffsetMilliseconds, int DurationMilliseconds);
