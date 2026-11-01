// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** Where a screen name came from; the board says so beside it, because nothing is captured of the screens themselves. */
export const screenEvidence = 'inferred from route';

const skipped = /^(v\d+|\{.*\})$/i;

const titleCase = (segment: string): string =>
    segment.split(/[-_]+/).filter(Boolean).map(word => word[0].toUpperCase() + word.slice(1).toLowerCase()).join(' ');

/**
 * The screen a command shape most likely belongs to: the first path segment after `/api`, title-cased
 * (`POST /api/book-tags/{id}` is "Book Tags"). Version segments and ids are not names.
 * @param shape A command shape, `METHOD /path`.
 * @returns The screen name, or undefined when the path has no segment to name it by.
 */
export const screenOfShape = (shape: string): string | undefined => {
    const path = shape.split(' ').slice(1).join(' ');
    const segments = path.split('/').filter(Boolean);
    const rest = segments[0]?.toLowerCase() === 'api' ? segments.slice(1) : segments;
    const name = rest.find(segment => !skipped.test(segment));
    return name ? titleCase(name) : undefined;
};

/**
 * The screen a slice belongs to: the one most of its shapes point at, the first when they tie.
 * @param shapes The command shapes of the slice.
 * @returns The screen name, or undefined when no shape names one.
 */
export const screenOfShapes = (shapes: readonly string[]): string | undefined => {
    const counts = new Map<string, number>();
    shapes.map(screenOfShape).forEach(screen => { if (screen) counts.set(screen, (counts.get(screen) ?? 0) + 1); });
    return [...counts.entries()].sort((left, right) => right[1] - left[1])[0]?.[0];
};
