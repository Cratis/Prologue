// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for the Prologue Receiver that correlated captures are sent to.
/// </summary>
public class ApiOptions
{
    /// <summary>
    /// Gets or sets the base address of the Prologue Receiver captures are posted to.
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:5005";
}
