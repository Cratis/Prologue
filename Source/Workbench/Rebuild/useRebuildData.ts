// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { ObserveRecentCaptures } from '../Captures/Live/RecentCaptures';
import { ObserveShapeCoverage } from '../Captures/Live/ShapeCoverage';
import { useDiscoveredShapes } from '../Capture/useDiscoveredShapes';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { ObserveRebuildDifferences } from './Differences/RebuildDifferences';
import { ObserveModelDrift } from './Drift/ModelDrift';
import { ObserveRecentParity } from './Parity/RecentParity';
import { ObserveRebuildBoard } from './RebuildBoard';

/**
 * The live data of the Rebuild page. Parity, differences and drift are projected on the server as events arrive; a prologue nothing
 * has happened for reads as empty lists, never as loading forever.
 */
export const useRebuildData = (prologue: PrologueDefinition) => {
    const parameters = { id: prologue.id };
    const [board] = ObserveRebuildBoard.use(parameters);
    const [differences] = ObserveRebuildDifferences.use(parameters);
    const [parity] = ObserveRecentParity.use(parameters);
    const [drift] = ObserveModelDrift.use(parameters);
    const [recent] = ObserveRecentCaptures.use(parameters);
    const [coverage] = ObserveShapeCoverage.use(parameters);
    const discovered = useDiscoveredShapes();

    const slices = board.data?.slices;
    const differenceList = differences.data?.differences;
    const observations = parity.data?.observations;
    const driftItems = drift.data?.items;
    const captures = recent.data?.captures;
    const shapes = coverage.data?.shapes;

    return useMemo(() => ({
        slices: slices ?? [],
        differences: differenceList ?? [],
        observations: observations ?? [],
        driftItems: driftItems ?? [],
        modelAccepted: drift.data?.accepted ?? false,
        captures: captures ?? [],
        knownShapes: [...new Set([...(shapes ?? []).map(shape => shape.shape), ...discovered])]
    }), [slices, differenceList, observations, driftItems, drift.data?.accepted, captures, shapes, discovered]);
};

export type RebuildData = ReturnType<typeof useRebuildData>;
