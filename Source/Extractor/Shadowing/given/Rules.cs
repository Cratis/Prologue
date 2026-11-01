// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Shadowing.given;

/// <summary>
/// Builds shadowing rules for specs.
/// </summary>
public static class Rules
{
    /// <summary>
    /// Builds a rule for a command shape.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="route">The route template, if the rule names one.</param>
    /// <param name="forward">The credential-bearing headers to forward.</param>
    /// <returns>The rule.</returns>
    public static ShadowRule For(string shape, string route = "", params string[] forward) =>
        new(new ShadowRuleOptions { Shape = shape, Route = route, Destination = "http://new-system:5000/", ForwardHeaders = [.. forward] });
}
#endif
