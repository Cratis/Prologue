// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents one shadowed command: when a request through the Extractor matches, a copy is sent to the shadow destination after
/// the legacy system has answered, and the two answers are compared.
/// </summary>
public class ShadowRuleOptions
{
    /// <summary>
    /// Gets or sets the command shape to shadow, such as <c language="csharp">POST /api/loans/{id}/return</c>, in the format the
    /// Workbench shows. Identities in the request path are generalized to <c language="csharp">{id}</c> before it is compared.
    /// </summary>
    public string Shape { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an ASP.NET route template to shadow, such as <c language="csharp">/api/loans/{id}/return</c>, matched against the
    /// request path. When both this and <see cref="Shape"/> are set, a request must match both.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base address of the shadow destination, the new system, such as <c language="csharp">http://new-system:5000/</c>.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of credential-bearing headers to forward to the shadow destination as well, such as
    /// <c language="csharp">Authorization</c>. Such headers are dropped unless they are listed here.
    /// </summary>
    public IList<string> ForwardHeaders { get; set; } = [];
}
