// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// The address of a repository host: a server, or an Azure DevOps organization.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ServerUrl(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// The address of github.com.
    /// </summary>
    public static readonly ServerUrl GitHub = new("https://github.com");

    /// <summary>
    /// Gets a value indicating whether this is an absolute http or https address.
    /// </summary>
    public bool IsValid => Uri.TryCreate(Value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ServerUrl"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ServerUrl(string value) => new(value);
}
