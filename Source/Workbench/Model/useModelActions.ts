// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useToast } from '../Components';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { AcceptModelForRebuild } from '../Rebuild/Drift/AcceptModelForRebuild';
import { AddRebuildSlice } from '../Rebuild/Slicing/AddRebuildSlice';
import { RebuildPlan } from './rebuildPlan';

const plural = (count: number, noun: string): string => `${count} ${noun}${count === 1 ? '' : 's'}`;

/** The commands of the Model page: accepting the model for rebuild and creating the Rebuild slices it holds. */
export const useModelActions = (prologue: PrologueDefinition, modelAccepted: boolean) => {
    const { run, isBusy } = useCommandRunner();
    const toast = useToast();

    /**
     * Accepts the model for rebuild unless it is accepted already, then adds the planned slices. Running it again adds nothing:
     * the plan leaves out what is tracked, and an accepted model is not accepted twice.
     */
    const acceptForRebuild = async (plan: RebuildPlan) => {
        if (!modelAccepted) {
            const accept = new AcceptModelForRebuild();
            accept.prologue = prologue.id;
            if (!await run('accept-model', accept)) return;
        }

        let added = 0;
        for (const slice of plan.create) {
            const command = new AddRebuildSlice();
            command.prologue = prologue.id;
            command.name = slice.name;
            command.shapes = [...slice.shapes];
            if (!await run(`add-slice-${slice.name}`, command)) return;
            added++;
        }

        toast.show(`The model is accepted for rebuild. ${plural(added, 'slice')} added to Rebuild${plan.existing.length > 0 ? `, ${plan.existing.length} already there` : ''}`);
    };

    return { acceptForRebuild, isBusy };
};
