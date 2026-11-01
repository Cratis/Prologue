// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useMemo } from 'react';
import { ObserveApplicationMap } from '../Discover/Map/ApplicationMap';
import { useCurrentPrologue } from '../Shell/shellState';
import { discoveredShapes } from './discoveredShapes';

/**
 * The command shapes Discover found in the code of the current prologue's systems, in the same format as a captured shape. Coverage
 * by route calls out the ones that were never captured, and the add-slice form of Rebuild offers them. Empty until the map answers.
 * @returns The shapes, the same array until the map changes.
 */
export const useDiscoveredShapes = (): readonly string[] => {
    const prologue = useCurrentPrologue();
    const [result] = ObserveApplicationMap.use({ prologue: prologue?.id ?? Guid.empty });
    const systems = result.hasData ? result.data?.systems : undefined;
    return useMemo(() => discoveredShapes(systems ? { systems } : undefined), [systems]);
};
