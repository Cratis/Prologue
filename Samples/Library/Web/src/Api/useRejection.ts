// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useState } from 'react';
import type { ProblemDetails } from './ProblemDetails';
import type { Rejection } from './Rejection';
import { toProblemDetails } from './toProblemDetails';

/**
 * Centralizes how a page performs a mutation. A 404, 409, or 422 is captured as problem details to
 * render, so a rejection never throws out of the render path or blanks the page.
 * @returns The rejection state and the way to perform a mutation.
 */
export const useRejection = (): Rejection => {
    const [problem, setProblem] = useState<ProblemDetails | undefined>(undefined);
    const [isPerforming, setIsPerforming] = useState(false);

    const perform = useCallback(async (action: () => Promise<void>) => {
        setIsPerforming(true);
        setProblem(undefined);

        try {
            await action();
        } catch (error) {
            setProblem(toProblemDetails(error));
        } finally {
            setIsPerforming(false);
        }
    }, []);

    return { problem, isPerforming, perform };
};
