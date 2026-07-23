// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Creates the <see cref="IChatClient"/> for a language-model configuration — the seam that lets sessions reach
/// the configured provider lazily and lets specs substitute the language model entirely.
/// </summary>
public interface IChatClientFactory
{
    /// <summary>
    /// Creates the chat client for the configured provider.
    /// </summary>
    /// <param name="options">The language-model options describing the provider to reach.</param>
    /// <returns>The <see cref="IChatClient"/> reaching the configured provider.</returns>
    IChatClient CreateFor(LlmOptions options);
}
