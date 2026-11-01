// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Finds the rule that shadows a request.
/// </summary>
/// <param name="rules">The configured rules.</param>
public class ShadowRouting(IEnumerable<ShadowRuleOptions> rules)
{
    readonly List<ShadowRule> _rules = [.. rules.Select(rule => new ShadowRule(rule))];

    /// <summary>
    /// Gets whether any request is shadowed at all.
    /// </summary>
    public bool Any => _rules.Count > 0;

    /// <summary>
    /// Finds the first rule that applies to a request.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <returns>The rule, or null when the request is not shadowed.</returns>
    public ShadowRule? Find(string method, PathString path) => _rules.Find(rule => rule.Matches(method, path));
}
