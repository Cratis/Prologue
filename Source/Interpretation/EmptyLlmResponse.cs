// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// The exception that is thrown when the language-model service returns an empty response body.
/// </summary>
/// <param name="endpoint">The endpoint that returned the empty response.</param>
public class EmptyLlmResponse(Uri endpoint) : Exception($"The language-model service at '{endpoint}' returned an empty response.");
