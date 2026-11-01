// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// Works out when two findings are the same finding, so that an analysis run again records what changed and nothing else.
/// </summary>
public static class FindingKeys
{
    /// <summary>
    /// Normalizes text so that case, punctuation and spacing do not make a finding look new.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The normalized text.</returns>
    public static string Normalize(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != ' ')
            {
                builder.Append(' ');
            }
        }

        return builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Gets the key of a capability.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="capability">The capability.</param>
    /// <returns>The key.</returns>
    public static string Capability(ModuleName module, CapabilityName capability) => $"{Normalize(module.Value)}|{Normalize(capability.Value)}";

    /// <summary>
    /// Gets the key of a decision point.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <param name="file">The file.</param>
    /// <returns>The key.</returns>
    public static string DecisionPoint(RuleText rule, SourcePath file) => $"{Normalize(rule.Value)}|{file.Value.ToLowerInvariant()}";

    /// <summary>
    /// Gets the key of a dependency.
    /// </summary>
    /// <param name="target">What is depended on.</param>
    /// <param name="kind">How.</param>
    /// <returns>The key.</returns>
    public static string Dependency(DependencyTarget target, DependencyKind kind) => $"{Normalize(target.Value)}|{kind}";

    /// <summary>
    /// Gets whether two sets of command shapes are the same set.
    /// </summary>
    /// <param name="left">One set.</param>
    /// <param name="right">The other.</param>
    /// <returns>True when both hold the same shapes, ignoring case and order.</returns>
    public static bool SameShapes(IEnumerable<RouteShape> left, IEnumerable<RouteShape> right) =>
        left.Select(shape => shape.Value.ToLowerInvariant()).ToHashSet().SetEquals(right.Select(shape => shape.Value.ToLowerInvariant()));
}
