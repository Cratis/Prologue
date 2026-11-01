// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// A shadowed command, ready to match requests against.
/// </summary>
public class ShadowRule
{
    static readonly string[] _mirrored = ["POST", "PUT", "PATCH", "DELETE"];

    readonly string _shape;
    readonly TemplateMatcher? _route;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowRule"/> class.
    /// </summary>
    /// <param name="options">The configured rule.</param>
    /// <exception cref="InvalidOperationException">The rule names neither a shape nor a route, or its destination is not an absolute address.</exception>
    public ShadowRule(ShadowRuleOptions options)
    {
        _shape = options.Shape.Trim();
        if (_shape.Length == 0 && string.IsNullOrWhiteSpace(options.Route))
        {
            throw new InvalidOperationException("A shadowing rule needs a shape or a route to match.");
        }

        if (!Uri.TryCreate(options.Destination, UriKind.Absolute, out var destination) || (destination.Scheme != Uri.UriSchemeHttp && destination.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"The shadowing destination '{options.Destination}' is not an absolute http or https address.");
        }

        Destination = destination;
        ForwardHeaders = new HashSet<string>(options.ForwardHeaders, StringComparer.OrdinalIgnoreCase);
        Label = _shape.Length > 0 ? _shape : options.Route.Trim();
        if (!string.IsNullOrWhiteSpace(options.Route))
        {
            var template = TemplateParser.Parse(options.Route.Trim());
            _route = new TemplateMatcher(template, []);
        }
    }

    /// <summary>
    /// Gets the base address requests are mirrored to.
    /// </summary>
    public Uri Destination { get; }

    /// <summary>
    /// Gets the credential-bearing headers that are forwarded as well.
    /// </summary>
    public ISet<string> ForwardHeaders { get; }

    /// <summary>
    /// Gets what the rule is called in logs.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets whether a request is a command that can be mirrored. Reads are never mirrored.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <returns>True for POST, PUT, PATCH and DELETE.</returns>
    public static bool IsMirrored(string method) => _mirrored.Contains(method, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether the rule applies to a request.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <returns>True when the request is a command and matches the shape and the route the rule names.</returns>
    public bool Matches(string method, PathString path) =>
        IsMirrored(method) &&
        (_shape.Length == 0 || string.Equals(CommandShapes.From(method, path.Value ?? string.Empty), Normalize(_shape), StringComparison.Ordinal)) &&
        (_route?.TryMatch(path, []) ?? true);

    static string Normalize(string shape)
    {
        var parts = shape.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? $"{parts[0].ToUpperInvariant()} {parts[1]}" : shape;
    }
}
