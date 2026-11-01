// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// The exception that is thrown when configuration assigns the same reverse-proxy cluster or OpenTelemetry service
/// name to more than one named service, so the extractor cannot tell which service its observations belong to.
/// </summary>
/// <param name="kind">What was claimed twice, for example <c language="csharp">cluster</c>.</param>
/// <param name="value">The value claimed twice.</param>
/// <param name="first">The first service claiming it.</param>
/// <param name="second">The second service claiming it.</param>
public class AmbiguousCaptureService(string kind, string value, ServiceName first, ServiceName second)
    : Exception($"The {kind} '{value}' is claimed by both the '{first.Value}' and the '{second.Value}' services. Assign it to one service only.");
