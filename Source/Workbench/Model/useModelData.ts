// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { ObserveAcceptedModel } from '../Events/Accepted/AcceptedModel';
import { ObserveCommandMatrix } from '../Explore/Matrix/CommandMatrix';
import { ObserveShapeCoverage } from '../Captures/Live/ShapeCoverage';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { ObserveModelDrift } from '../Rebuild/Drift/ModelDrift';
import { ObserveRebuildBoard } from '../Rebuild/RebuildBoard';
import { layoutBoard } from './board';
import { ObserveModelScreenplay } from './Export/ModelScreenplay';

/**
 * What the Model page shows for a prologue. The model, its Screenplay and the evidence behind each slice are projected on the server
 * and arrive as suggestions are accepted or rejected, so nothing here is refreshed by hand.
 */
export const useModelData = (prologue: PrologueDefinition) => {
    const parameters = { id: prologue.id };
    const [model] = ObserveAcceptedModel.use(parameters);
    const [screenplay] = ObserveModelScreenplay.use(parameters);
    const [matrix] = ObserveCommandMatrix.use(parameters);
    const [coverage] = ObserveShapeCoverage.use(parameters);
    const [rebuild] = ObserveRebuildBoard.use(parameters);
    const [drift] = ObserveModelDrift.use(parameters);

    const modelData = model.data;
    const shapes = matrix.data?.shapes;
    const covered = coverage.data?.shapes;
    const tracked = rebuild.data?.slices;

    return useMemo(() => ({
        ready: model.isReady,
        model: modelData,
        board: layoutBoard(modelData),
        play: screenplay.data?.play ?? '',
        drafts: screenplay.data?.drafts ?? 0,
        matrix: shapes ?? [],
        counts: new Map((covered ?? []).map(entry => [entry.shape, entry.captures] as const)),
        tracked: tracked ?? [],
        modelAccepted: drift.data?.accepted ?? false
    }), [model.isReady, modelData, screenplay.data?.play, screenplay.data?.drafts, shapes, covered, tracked, drift.data?.accepted]);
};

export type ModelData = ReturnType<typeof useModelData>;
