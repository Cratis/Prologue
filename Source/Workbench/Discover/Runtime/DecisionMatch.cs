// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// How a decision point in the code matches the runtime evidence.
/// </summary>
/// <param name="Happened">How often it happened.</param>
/// <param name="Status">The HTTP status it happened with most; zero when it has none.</param>
/// <param name="Runtime">Whether the evidence matches it.</param>
public record DecisionMatch(int Happened, int Status, DecisionRuntime Runtime);
