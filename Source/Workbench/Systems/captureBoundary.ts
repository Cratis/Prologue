// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PrologueDefinition } from '../Prologues/PrologueDefinition';

/** What the capture boundary says is never recorded, whatever the sources. */
export const neverRecorded: readonly string[] = [
    'Row values',
    'HTTP bodies',
    'GET and other reads',
    'Log bodies, metric values',
    'Non-allowlisted attribute values',
    'Write access to anything'
];

/** What the prologue's evidence steps and runtime sources record, in the words of the capture boundary. */
export const recordedItems = (prologue: PrologueDefinition): string[] => {
    const systems = prologue.systems;
    const http = systems.some(system => system.httpClusters.length > 0);
    const database = systems.some(system => system.databases.length > 0);
    const telemetry = systems.some(system => system.telemetryServices.length > 0);
    const keys = [...new Set(systems.flatMap(system => system.attributeKeys))];

    return [
        prologue.steps.capture && http && 'Method, path with query string, status, trace id',
        prologue.steps.capture && database && 'Changed table and column names per transaction',
        prologue.steps.capture && database && 'Schema: keys, foreign keys, unique constraints',
        prologue.steps.capture && database && 'Read from the database transaction log, never by querying tables',
        prologue.steps.capture && telemetry && 'Span names, durations, attribute keys',
        prologue.steps.capture && telemetry && keys.length > 0 && `Values of ${keys.join(', ')}`,
        prologue.steps.discover && 'Source code, cloned read-only into your network'
    ].filter((item): item is string => typeof item === 'string');
};
