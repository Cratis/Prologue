// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';
import { RequestFailed } from './RequestFailed';

/**
 * Turns anything that was thrown into problem details, so a rejection and a transport failure both end
 * up in the same message block rather than blanking the page.
 * @param error The value that was thrown.
 * @returns The problem details to render.
 */
export const toProblemDetails = (error: unknown): ProblemDetails => {
    if (error instanceof RequestFailed) {
        return error.problem;
    }

    return {
        title: 'The request could not be completed',
        detail: error instanceof Error ? error.message : String(error)
    };
};
