// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ExtractedSliceType } from '../ExtractedSliceType';
import { AcceptedEvent } from '../Events/Accepted/AcceptedEvent';
import { AcceptedModel } from '../Events/Accepted/AcceptedModel';
import { AcceptedSlice } from '../Events/Accepted/AcceptedSlice';
import { screenOfShapes } from './screens';

/** One column of the board: a slice with what each row shows for it. */
export interface BoardSlice {
    readonly key: string;
    readonly module: string;
    readonly feature: string;
    readonly name: string;
    readonly type: ExtractedSliceType;
    readonly isOpen: boolean;
    readonly screen?: string;
    readonly commands: readonly string[];
    readonly events: readonly AcceptedEvent[];
    readonly readModels: readonly string[];
    readonly slice: AcceptedSlice;
}

/** The slices of one module, side by side. */
export interface BoardModule {
    readonly name: string;
    readonly slices: readonly BoardSlice[];
}

/** The board: modules as column groups, one column per slice, and a row of events for each stream. */
export interface Board {
    readonly modules: readonly BoardModule[];
    readonly slices: readonly BoardSlice[];
    readonly streams: readonly string[];
}

/** The label a slice type carries in a column header. */
export const sliceTypeLabel = (type: ExtractedSliceType): string => {
    switch (type) {
        case ExtractedSliceType.stateView: return 'View';
        case ExtractedSliceType.automation: return 'Automation';
        case ExtractedSliceType.translation: return 'Translation';
        default: return 'Change';
    }
};

/** The key of a slice, unique across the board even when two features reuse a slice name. */
export const sliceKey = (module: string, feature: string, name: string): string => `${module}/${feature}/${name}`;

/**
 * Lays the accepted model out as a board.
 * @param model The accepted model.
 * @returns The modules in model order, each with its slices as columns, and the streams in the order they first appear.
 */
export const layoutBoard = (model: AcceptedModel | undefined): Board => {
    const modules = (model?.modules ?? []).map(module => ({
        name: module.name,
        slices: module.features.flatMap(feature => feature.slices.map(slice => ({
            key: sliceKey(module.name, feature.name, slice.name),
            module: module.name,
            feature: feature.name,
            name: slice.name,
            type: slice.type,
            isOpen: slice.isOpen,
            screen: screenOfShapes(slice.shapes),
            commands: slice.commands.map(command => command.name),
            events: slice.events,
            readModels: slice.readModels.map(readModel => readModel.name),
            slice
        })))
    })).filter(module => module.slices.length > 0);

    const slices = modules.flatMap(module => module.slices);
    return { modules, slices, streams: [...new Set(slices.flatMap(slice => slice.events.map(event => event.stream)))] };
};

/** The events of a column that belong to one stream. */
export const eventsOfStream = (slice: BoardSlice, stream: string): readonly AcceptedEvent[] => slice.events.filter(event => event.stream === stream);

/** How many slices, accepted events and open events the board holds, for its stamp. */
export const boardSummary = (board: Board): string => {
    const events = board.slices.flatMap(slice => slice.events);
    const open = events.filter(event => event.isOpen).length;
    return `${board.slices.length} slices · ${events.length - open} accepted events · ${open} open`;
};
