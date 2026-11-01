// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { ApplicationMap } from './Map/ApplicationMap';
import { MapCapability } from './Map/MapCapability';

export interface SelectedCapability {
    readonly key: string;
    readonly capability: MapCapability;
}

/**
 * Which capability of the map is shown in the detail panel. A selection that the map no longer holds, such as after a
 * re-analysis removed it, is dropped rather than shown stale.
 * @param map The application map.
 * @returns The selected capability and how to select one.
 */
export const useCapabilitySelection = (map: ApplicationMap | undefined) => {
    const [selected, select] = useState<SelectedCapability | undefined>();

    const current = selected && map?.systems.some(system => system.modules.some(module => module.capabilities.some(capability =>
        `${String(system.subject)}|${module.name}|${capability.capability}` === selected.key)))
        ? selected
        : undefined;

    return { selected: current, select };
};
