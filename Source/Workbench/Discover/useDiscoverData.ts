// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { AnalysisStatus, ObserveAnalysisStatuses } from './Map/AnalysisStatus';
import { ApplicationMap, ObserveApplicationMap } from './Map/ApplicationMap';
import { CodeDecisionPoint, ObserveDecisionPoints } from './Map/CodeDecisionPoint';
import { CodeDependency, ObserveDependencies } from './Map/CodeDependency';

export interface DiscoverMap {
    /** False until the application map has answered, so the page does not show an empty answer a pending query could contradict. */
    readonly loaded: boolean;
    readonly map?: ApplicationMap;
    readonly decisionPoints: readonly CodeDecisionPoint[];
    readonly dependencies: readonly CodeDependency[];
}

/**
 * Observes what Discover shows for a prologue: the application map, its decision points and its dependencies. The shared,
 * read-only page reads the same three.
 * @param prologue The prologue.
 * @returns The three answers.
 */
export const useDiscoverMap = (prologue: Guid): DiscoverMap => {
    const [map] = ObserveApplicationMap.use({ prologue });
    const [decisionPoints] = ObserveDecisionPoints.use({ prologue });
    const [dependencies] = ObserveDependencies.use({ prologue });

    return {
        loaded: map.hasData && decisionPoints.hasData && dependencies.hasData,
        map: map.hasData ? map.data : undefined,
        decisionPoints: decisionPoints.data,
        dependencies: dependencies.data
    };
};

/**
 * Observes the analysis status of every system and analyzed folder of a prologue.
 * @param prologue The prologue.
 * @returns The statuses.
 */
export const useAnalysisStatuses = (prologue: Guid): readonly AnalysisStatus[] => {
    const [statuses] = ObserveAnalysisStatuses.use({ prologue });
    return statuses.data;
};
