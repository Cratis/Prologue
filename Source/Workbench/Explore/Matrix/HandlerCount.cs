// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// How often a span was the first one observed for a command shape, which is the handler that served it.
/// </summary>
/// <param name="Span">The span.</param>
/// <param name="Captures">How many captures began with it.</param>
public record HandlerCount(SpanName Span, int Captures);
