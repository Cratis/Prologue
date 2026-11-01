// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ApplicationMap } from '../Discover/Map/ApplicationMap';

/**
 * The command shapes of every capability the application map holds, each once, in the order the map lists them. Shapes use the
 * format captures use: the method, the path, and identifying segments as {id}.
 * @param map The application map of the prologue, or undefined while it has not answered.
 * @returns The shapes the code is known to serve.
 */
export const discoveredShapes = (map: Pick<ApplicationMap, 'systems'> | undefined): readonly string[] => {
    const shapes = (map?.systems ?? [])
        .flatMap(system => system.modules ?? [])
        .flatMap(module => module.capabilities ?? [])
        .flatMap(capability => capability.shapes ?? [])
        .filter(shape => shape.length > 0);
    return [...new Set(shapes)];
};
