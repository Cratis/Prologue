// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Extraction;

/// <summary>
/// Maps the routes found in code onto the command shapes the runtime captures, so a capability in the code can be matched to
/// what the running system did. The format is the one captures use: "METHOD /path" with the method upper-cased, no query
/// string and no trailing slash, and every identifying segment written as "{id}".
/// </summary>
public static class RouteTemplate
{
    /// <summary>
    /// Builds the shape of an action from the route attributes around it.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="controllerRoute">The route template on the controller, when it has one.</param>
    /// <param name="actionTemplate">The template on the action's method attribute or route attribute, when it has one.</param>
    /// <param name="controller">The controller's name without the suffix, which fills "[controller]".</param>
    /// <param name="action">The action's name, which fills "[action]".</param>
    /// <returns>The shape.</returns>
    public static RouteShape Shape(string method, string? controllerRoute, string? actionTemplate, string controller, string action)
    {
        var prefix = Fill(controllerRoute ?? string.Empty, controller, action);
        var template = Fill(actionTemplate ?? string.Empty, controller, action);
        var combined = template.StartsWith('/') || template.StartsWith("~/", StringComparison.Ordinal)
            ? template.TrimStart('~')
            : $"{prefix.Trim('/')}/{template.Trim('/')}";
        return Normalize(method, combined.Replace("?}", "}", StringComparison.Ordinal));
    }

    /// <summary>
    /// Normalizes a method and path, such as one an agent reported, to a shape.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path, with or without a query string.</param>
    /// <returns>The shape.</returns>
    public static RouteShape Normalize(string method, string path)
    {
        var withoutQuery = path.Split('?', '#')[0];
        var segments = withoutQuery.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Segment);
        return new($"{method.Trim().ToUpperInvariant()} /{string.Join('/', segments)}");
    }

    /// <summary>
    /// Reads a shape written as text, such as "post /api/loans/12/return".
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The shape, or null when the text is not a method followed by a path.</returns>
    public static RouteShape? Parse(string text)
    {
        var parts = text.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 && parts[0].All(char.IsLetter) && parts[1].StartsWith('/') ? Normalize(parts[0], parts[1]) : null;
    }

    static string Fill(string template, string controller, string action) =>
        template.Replace("[controller]", controller.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)
            .Replace("[action]", action.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);

    static string Segment(string segment)
    {
        if (segment.Contains('{', StringComparison.Ordinal))
        {
            return "{id}";
        }

        var identifying = segment.All(char.IsDigit) || Guid.TryParse(segment, out _) || (segment.Length >= 4 && segment.Any(char.IsDigit));
        return identifying ? "{id}" : segment;
    }
}
