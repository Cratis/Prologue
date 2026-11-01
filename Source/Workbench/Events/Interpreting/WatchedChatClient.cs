// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// A chat client that says why a call failed before it lets the failure through, so the interpretation can fall back to the heuristic model and still tell the person.
/// </summary>
/// <param name="inner">The chat client calls go to.</param>
/// <param name="failed">Told why a call failed.</param>
public class WatchedChatClient(IChatClient inner, Action<string> failed) : DelegatingChatClient(inner)
{
    /// <summary>
    /// Describes a failure for a person: the exception message, without the exception's type or stack.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The description.</returns>
    public static string Describe(Exception exception) =>
        string.IsNullOrWhiteSpace(exception.Message) ? "The Interpreter agent could not be reached" : exception.Message;

    /// <inheritdoc/>
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.GetResponseAsync(messages, options, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            failed(Describe(exception));
            throw;
        }
    }
}
