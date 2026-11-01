// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Decides, before the legacy system is called, whether a request body can be copied for the shadow destination.
/// </summary>
public enum BodyPlan
{
    /// <summary>
    /// The request has no body.
    /// </summary>
    None,

    /// <summary>
    /// The body is small enough and its size is known, so it can be buffered while the legacy system reads it.
    /// </summary>
    Buffer,

    /// <summary>
    /// The body is larger than <see cref="ShadowLimits.MaximumBodyBytes"/> or its size is not known up front, so it is not buffered.
    /// </summary>
    Skip
}

/// <summary>
/// Plans how a request body is copied.
/// </summary>
public static class BodyPlans
{
    /// <summary>
    /// Plans the copy of a body. Buffering a body of unknown size could fail the request that is being forwarded, so such a body is skipped.
    /// </summary>
    /// <param name="contentLength">The Content-Length of the request, or null when it has none.</param>
    /// <param name="chunked">Whether the request uses a transfer encoding, so its size is not known up front.</param>
    /// <returns>The plan.</returns>
    public static BodyPlan For(long? contentLength, bool chunked)
    {
        if (chunked || contentLength > ShadowLimits.MaximumBodyBytes)
        {
            return BodyPlan.Skip;
        }

        return contentLength is null or 0 ? BodyPlan.None : BodyPlan.Buffer;
    }
}
