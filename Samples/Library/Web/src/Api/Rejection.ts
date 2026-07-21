// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';

/**
 * Represents the outcome of the mutations a page performs, capturing rejections rather than letting
 * them escape.
 */
export interface Rejection {
    /** The problem the last mutation was rejected with, if it was. */
    problem?: ProblemDetails;

    /** Whether a mutation is in flight. */
    isPerforming: boolean;

    /** Performs a mutation, capturing any rejection it comes back with. */
    perform: (action: () => Promise<void>) => Promise<void>;
}
