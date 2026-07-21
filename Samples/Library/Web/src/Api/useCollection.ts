// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useEffect, useState } from 'react';
import { apiClient } from './apiClient';
import type { Collection } from './Collection';
import type { ProblemDetails } from './ProblemDetails';
import { toProblemDetails } from './toProblemDetails';

/**
 * Loads a collection from the API on mount and exposes a way to load it again — which is what every
 * page does after a mutation, since this frontend re-fetches rather than patching state locally.
 * @param path The path of the collection, relative to the API base URL.
 * @returns The collection and its load state.
 */
export const useCollection = <TItem>(path: string): Collection<TItem> => {
    const [items, setItems] = useState<TItem[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [problem, setProblem] = useState<ProblemDetails | undefined>(undefined);

    const reload = useCallback(async () => {
        setIsLoading(true);

        try {
            setItems(await apiClient.get<TItem[]>(path));
            setProblem(undefined);
        } catch (error) {
            setProblem(toProblemDetails(error));
        } finally {
            setIsLoading(false);
        }
    }, [path]);

    useEffect(() => {
        void reload();
    }, [reload]);

    return { items, isLoading, problem, reload };
};
