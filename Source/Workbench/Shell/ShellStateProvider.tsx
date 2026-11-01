// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode, useCallback, useMemo, useState } from 'react';
import { EvidenceStep } from '../Prologues/EvidenceStep';
import { TurnEvidenceStepOff } from '../Prologues/Evidence/TurnEvidenceStepOff';
import { TurnEvidenceStepOn } from '../Prologues/Evidence/TurnEvidenceStepOn';
import { ObservePrologues } from '../Prologues/PrologueDefinition';
import { defaultOptionalOn, OptionalStepState, PrologueSummary, ShellState, ShellStateContext } from './shellState';
import { StepId } from './steps';
import { useCommandRunner } from './useCommandRunner';

const storageKey = 'prologue.selected';

const evidenceSteps: Partial<Record<StepId, EvidenceStep>> = {
    discover: EvidenceStep.discover,
    capture: EvidenceStep.capture,
    rebuild: EvidenceStep.rebuild
};

const remembered = () => {
    try {
        return window.localStorage.getItem(storageKey) ?? '';
    } catch {
        return '';
    }
};

/** Provides the shell state from the observed prologues; evidence steps change through commands, so the server's rules apply. */
export const ShellStateProvider = ({ children }: { children: ReactNode }) => {
    const [result] = ObservePrologues.use();
    const [chosenId, setChosenId] = useState(remembered);
    const [creating, setCreating] = useState(false);
    const { run } = useCommandRunner();

    const definitions = result.data;
    const definition = definitions.find(candidate => String(candidate.id) === chosenId) ?? definitions[0];
    const selectedId = definition ? String(definition.id) : '';

    const prologues = useMemo<readonly PrologueSummary[]>(() => definitions.map(candidate => ({
        id: String(candidate.id),
        name: candidate.name,
        system: candidate.systems.length > 0 ? candidate.systems.map(system => system.name).join(', ') : 'No systems yet'
    })), [definitions]);

    const selectPrologue = useCallback((id: string) => {
        setChosenId(id);
        setCreating(false);
        try {
            window.localStorage.setItem(storageKey, id);
        } catch {
            // Remembering the choice is a convenience; a browser that refuses storage still works.
        }
    }, []);

    const setOptionalStep = useCallback((id: StepId, on: boolean) => {
        const step = evidenceSteps[id];
        if (step === undefined || !definition) return;

        const command = on ? new TurnEvidenceStepOn() : new TurnEvidenceStepOff();
        command.prologue = definition.id;
        command.step = step;
        void run(`step-${id}`, command, on ? 'Step turned on' : 'Step turned off');
    }, [definition, run]);

    const optionalOn = useMemo<OptionalStepState>(() => definition
        ? { discover: definition.steps.discover, capture: definition.steps.capture, rebuild: definition.steps.rebuild }
        : defaultOptionalOn, [definition]);

    const value = useMemo<ShellState>(() => ({
        loaded: result.isReady,
        prologues,
        definition,
        selectedId,
        selectPrologue,
        creating,
        setCreating,
        optionalOn,
        setOptionalStep
    }), [result.isReady, prologues, definition, selectedId, selectPrologue, creating, optionalOn, setOptionalStep]);

    return <ShellStateContext.Provider value={value}>{children}</ShellStateContext.Provider>;
};
