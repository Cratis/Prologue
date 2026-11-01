// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// What a system decided about a command, judged by the status it answered with.
/// </summary>
public enum HttpOutcome
{
    /// <summary>
    /// The answer is neither a success nor a refusal, so nothing was decided.
    /// </summary>
    NotDecided,

    /// <summary>
    /// The system accepted the command: it answered with a success or a redirect (2xx, 3xx).
    /// </summary>
    Accepted,

    /// <summary>
    /// The system refused the command: it answered with a client or server error (4xx, 5xx).
    /// </summary>
    Refused
}

/// <summary>
/// Classifies the status a command was answered with.
/// </summary>
public static class HttpOutcomes
{
    /// <summary>
    /// Classifies a status: 200-399 accept the command, 400-599 refuse it, anything else decides nothing.
    /// </summary>
    /// <param name="status">The HTTP status code.</param>
    /// <returns>The outcome.</returns>
    public static HttpOutcome Classify(int status) => status switch
    {
        >= 200 and < 400 => HttpOutcome.Accepted,
        >= 400 and < 600 => HttpOutcome.Refused,
        _ => HttpOutcome.NotDecided
    };
}
