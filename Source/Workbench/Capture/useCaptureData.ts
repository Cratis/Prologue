// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { ObserveCaptureAggregates } from '../Captures/Live/CaptureAggregates';
import { ObserveRecentCaptures } from '../Captures/Live/RecentCaptures';
import { ObserveShapeCoverage } from '../Captures/Live/ShapeCoverage';
import { ObserveBusinessDecisions } from '../Decisions/BusinessDecision';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { ObserveScenarioScript } from '../Scenarios/ScenarioScript';
import { BucketLike } from './captureTypes';

/** The live numbers of the Capture page; a prologue nothing has arrived for reads as zeros and empty lists, never as loading forever. */
export const useCaptureData = (prologue: PrologueDefinition) => {
    const parameters = { id: prologue.id };
    const [recent] = ObserveRecentCaptures.use(parameters);
    const [aggregates] = ObserveCaptureAggregates.use(parameters);
    const [coverage] = ObserveShapeCoverage.use(parameters);
    const [decisions] = ObserveBusinessDecisions.use(parameters);
    const [script] = ObserveScenarioScript.use(parameters);

    const captures = recent.data?.captures;
    const buckets = aggregates.data?.perMinute;
    const scenarios = script.data?.scenarios;
    const decisionList = decisions.data;
    const shapes = coverage.data?.shapes;

    return useMemo(() => ({
        ready: recent.isReady && aggregates.isReady,
        captures: captures ?? [],
        decisions: Array.isArray(decisionList) ? decisionList : [],
        shapes: shapes ?? [],
        total: aggregates.data?.total ?? 0,
        withCommand: aggregates.data?.withCommand ?? 0,
        traceLinked: aggregates.data?.traceLinked ?? 0,
        uncorrelated: aggregates.data?.uncorrelatedTransactions ?? 0,
        tables: aggregates.data?.tables ?? [],
        buckets: (buckets ?? []) as BucketLike[],
        scenarioCounts: aggregates.data?.scenarios ?? [],
        scenarios: scenarios ?? [],
        activeScenario: script.data?.active ?? ''
    }), [recent.isReady, aggregates.isReady, aggregates.data, captures, buckets, scenarios, decisionList, shapes, script.data?.active]);
};

export type CaptureData = ReturnType<typeof useCaptureData>;
