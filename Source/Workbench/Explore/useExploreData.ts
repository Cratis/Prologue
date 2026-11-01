// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { ObserveSuggestedQuestions } from './Asking/SuggestedQuestion';
import { ObserveJourneys } from './Journeys/Journeys';
import { ObserveCommandMatrix } from './Matrix/CommandMatrix';
import { ObserveInterpretationScope } from './Scope/InterpretationScope';
import { ObserveUncorrelatedWrites } from './Uncorrelated/UncorrelatedWrites';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';

/** What the Explore page shows for a prologue; a prologue nothing has arrived for reads as empty lists, never as loading forever. */
export const useExploreData = (prologue: PrologueDefinition) => {
    const parameters = { id: prologue.id };
    const [matrix] = ObserveCommandMatrix.use(parameters);
    const [uncorrelated] = ObserveUncorrelatedWrites.use(parameters);
    const [journeys] = ObserveJourneys.use(parameters);
    const [scope] = ObserveInterpretationScope.use(parameters);
    const [suggestions] = ObserveSuggestedQuestions.use(parameters);

    const shapes = matrix.data?.shapes;
    const tables = uncorrelated.data?.tables;
    const sequences = journeys.data?.sequences;
    const excluded = scope.data?.excluded;
    const split = scope.data?.split;
    const suggested = suggestions.data;

    return useMemo(() => ({
        ready: matrix.isReady,
        shapes: shapes ?? [],
        uncorrelated: tables ?? [],
        sequences: sequences ?? [],
        excluded: new Set<string>(excluded ?? []),
        split: new Set<string>(split ?? []),
        suggestions: Array.isArray(suggested) ? suggested : []
    }), [matrix.isReady, shapes, tables, sequences, excluded, split, suggested]);
};

export type ExploreData = ReturnType<typeof useExploreData>;
