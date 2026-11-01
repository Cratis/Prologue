// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { MarkScenarioEnded } from '../Scenarios/Marking/MarkScenarioEnded';
import { MarkScenarioStarted } from '../Scenarios/Marking/MarkScenarioStarted';
import { AddScenario } from '../Scenarios/Scripting/AddScenario';
import { RemoveScenario } from '../Scenarios/Scripting/RemoveScenario';
import { useCommandRunner } from '../Shell/useCommandRunner';

/**
 * Marks the start and end of scenarios and edits the script of a prologue. The server refuses an overlapping start and a duplicate,
 * and the refusal is shown as a toast.
 */
export const useScenarioMarking = (prologue: PrologueDefinition, activeScenario: string) => {
    const [typed, setTyped] = useState('');
    const [adding, setAdding] = useState('');
    const { run, isBusy } = useCommandRunner();

    const markStart = async () => {
        const command = new MarkScenarioStarted();
        command.prologue = prologue.id;
        command.name = typed;
        if (await run('mark-scenario', command, `Captures are now labeled \u201c${typed.trim()}\u201d`)) setTyped('');
    };

    const markEnd = async () => {
        const command = new MarkScenarioEnded();
        command.prologue = prologue.id;
        await run('mark-scenario', command, `Marked end of \u201c${activeScenario}\u201d`);
    };

    const add = async () => {
        const command = new AddScenario();
        command.prologue = prologue.id;
        command.name = adding;
        if (await run('add-scenario', command)) setAdding('');
    };

    const remove = async (name: string) => {
        const command = new RemoveScenario();
        command.prologue = prologue.id;
        command.name = name;
        await run(`remove-scenario-${name}`, command);
    };

    return { typed, setTyped, adding, setAdding, markStart, markEnd, add, remove, isBusy };
};
