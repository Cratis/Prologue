// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';

/**
 * Represents a collection loaded from the API, together with how the load went.
 */
export interface Collection<TItem> {
    /** The items that were loaded. */
    items: TItem[];

    /** Whether a load is in flight. */
    isLoading: boolean;

    /** The problem the load reported, if it did not succeed. */
    problem?: ProblemDetails;

    /** Loads the collection again. */
    reload: () => Promise<void>;
}
