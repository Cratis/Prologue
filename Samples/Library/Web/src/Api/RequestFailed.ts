// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';

/**
 * Represents a request the API did not accept, carrying the problem details it reported.
 */
export class RequestFailed extends Error {
    /** The problem details the API reported. */
    readonly problem: ProblemDetails;

    /**
     * Initializes a new instance of {@link RequestFailed}.
     * @param problem The problem details the API reported.
     */
    constructor(problem: ProblemDetails) {
        super(problem.title ?? 'The request failed');
        this.name = 'RequestFailed';
        this.problem = problem;
    }
}
