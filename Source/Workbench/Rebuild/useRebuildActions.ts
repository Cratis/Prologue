// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { AcceptDifference } from './Differences/AcceptDifference';
import { MatchLegacyForDifference } from './Differences/MatchLegacyForDifference';
import { AcceptModelForRebuild } from './Drift/AcceptModelForRebuild';
import { AcknowledgeDrift } from './Drift/AcknowledgeDrift';
import { AddRebuildSlice } from './Slicing/AddRebuildSlice';
import { RemoveRebuildSlice } from './Slicing/RemoveRebuildSlice';
import { CutOverSlice } from './Staging/CutOverSlice';
import { MarkRebuildSliceBuilt } from './Staging/MarkRebuildSliceBuilt';
import { MarkRebuildSliceModeled } from './Staging/MarkRebuildSliceModeled';
import { StartShadowing } from './Staging/StartShadowing';
import { StepRebuildSliceBack } from './Staging/StepRebuildSliceBack';
import { SliceAction } from './stages';

type SliceCommand = MarkRebuildSliceModeled | MarkRebuildSliceBuilt | StartShadowing | CutOverSlice | StepRebuildSliceBack | RemoveRebuildSlice;

const forSlice: Record<SliceAction, { create: () => SliceCommand; done: (name: string) => string }> = {
    model: { create: () => new MarkRebuildSliceModeled(), done: name => `${name} is part of the model` },
    build: { create: () => new MarkRebuildSliceBuilt(), done: name => `${name} is built` },
    shadow: { create: () => new StartShadowing(), done: name => `${name} is shadowing: its commands are mirrored and compared` },
    cutover: { create: () => new CutOverSlice(), done: name => `${name} now serves its route` }
};

/** The commands of the Rebuild page. A refusal, such as cutting over with unresolved differences, is shown as a toast by the runner. */
export const useRebuildActions = (prologue: PrologueDefinition) => {
    const { run, isBusy } = useCommandRunner();

    const forName = <T extends SliceCommand>(command: T, name: string): T => {
        command.prologue = prologue.id;
        command.name = name;
        return command;
    };

    const advance = (action: SliceAction, name: string) => run(`slice-${name}`, forName(forSlice[action].create(), name), forSlice[action].done(name));
    const stepBack = (name: string) => run(`slice-${name}`, forName(new StepRebuildSliceBack(), name), `${name} stepped back`);
    const remove = (name: string) => run(`slice-${name}`, forName(new RemoveRebuildSlice(), name), `${name} is no longer tracked`);

    const add = async (name: string, shapes: string[]) => {
        const command = new AddRebuildSlice();
        command.prologue = prologue.id;
        command.name = name;
        command.shapes = shapes;
        return run('add-slice', command, `${name.trim()} added`);
    };

    const resolve = async (observation: Guid, accept: boolean) => {
        const command = accept ? new AcceptDifference() : new MatchLegacyForDifference();
        command.prologue = prologue.id;
        command.observation = observation;
        return run(`difference-${String(observation)}`, command, accept ? 'Difference accepted and recorded as a decision' : 'The slice will be changed to match legacy');
    };

    const acceptModel = async () => {
        const command = new AcceptModelForRebuild();
        command.prologue = prologue.id;
        return run('accept-model', command, 'The model is accepted. What the legacy system does from now on is checked against it');
    };

    const suggestEvents = async (key: string) => {
        const command = new AcknowledgeDrift();
        command.prologue = prologue.id;
        command.key = key;
        return run(`drift-${key}`, command, 'Events are to be suggested for this. Review them in Events');
    };

    return { advance, stepBack, remove, add, resolve, acceptModel, suggestEvents, isBusy };
};
