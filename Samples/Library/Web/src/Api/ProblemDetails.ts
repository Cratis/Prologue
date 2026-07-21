// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents the RFC 7807 body the API returns when it rejects a request. A rejection is a normal,
 * meaningful outcome to render, not a crash.
 */
export interface ProblemDetails {
    /** The short, human-readable summary of the problem. */
    title?: string;

    /** The explanation specific to this occurrence of the problem. */
    detail?: string;

    /** The HTTP status code the problem was reported with. */
    status?: number;

    /** The URI reference identifying the problem type. */
    type?: string;

    /** The URI reference identifying the specific occurrence. */
    instance?: string;
}
