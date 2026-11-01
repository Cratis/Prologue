// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, useContext } from 'react';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { Posture } from '../AI/networkPosture';
import { useAIPosture } from '../AI/useAIPosture';
import { StepId } from './steps';

/** A prologue as the switcher shows it. */
export interface PrologueSummary {
    readonly id: string;
    readonly name: string;

    /** The names of the prologue's systems, for the line under its name. */
    readonly system: string;
}

/** The optional steps that can be switched off, by step id. */
export type OptionalStepState = Readonly<Partial<Record<StepId, boolean>>>;

export interface ShellState {
    /** False until the first answer about which prologues exist has arrived. */
    readonly loaded: boolean;
    readonly prologues: readonly PrologueSummary[];

    /** The selected prologue's full definition: its systems, evidence steps, correlation and output. Undefined when there is none. */
    readonly definition?: PrologueDefinition;
    readonly selectedId: string;
    readonly selectPrologue: (id: string) => void;

    /** True while the user is defining a new prologue, even when others exist. */
    readonly creating: boolean;
    readonly setCreating: (creating: boolean) => void;
    readonly optionalOn: OptionalStepState;
    readonly setOptionalStep: (id: StepId, on: boolean) => void;
}

/** What the switcher and the page eyebrows show while no prologue exists yet. */
export const noPrologue: PrologueSummary = { id: '', name: 'No prologue yet', system: '' };

export const defaultOptionalOn: OptionalStepState = { discover: true, capture: true, rebuild: true };

export const ShellStateContext = createContext<ShellState>({
    loaded: false,
    prologues: [],
    selectedId: '',
    selectPrologue: () => undefined,
    creating: false,
    setCreating: () => undefined,
    optionalOn: defaultOptionalOn,
    setOptionalStep: () => undefined
});

/** The prologues to choose from and the selected one; feeds the sidebar switcher and every page eyebrow. */
export const usePrologues = () => {
    const { prologues, selectedId, selectPrologue, loaded, creating, setCreating } = useContext(ShellStateContext);
    const current = prologues.find(prologue => prologue.id === selectedId) ?? prologues[0] ?? noPrologue;
    return { prologues, current, selectPrologue, loaded, hasPrologue: prologues.length > 0, creating, setCreating };
};

/**
 * The selected prologue's definition: its `id`, `name`, `systems` (each with its `location` of host, repository, branch and sub path),
 * `steps`, `correlation` and `output`. Undefined while loading or when no prologue exists. Pages that work per system read it from here.
 */
export const useCurrentPrologue = (): PrologueDefinition | undefined => useContext(ShellStateContext).definition;

/** Whether optional steps are on for the selected prologue, and the means to change that. A refusal is shown to the user as a toast. */
export const useOptionalSteps = () => {
    const { optionalOn, setOptionalStep } = useContext(ShellStateContext);
    const isOn = (id: StepId) => optionalOn[id] ?? true;
    return { isOn, setOn: setOptionalStep };
};

/** Counts shown as a badge next to a step in the sidebar, such as open questions on Events. */
export const useStepBadges = (): Readonly<Partial<Record<StepId, number>>> => ({});

export interface CaptureStatus {
    readonly capturing: boolean;
    readonly elapsed: string;
    readonly localOnly: boolean;

    /** How far the AI work leaves the customer's network. */
    readonly posture: Posture;
}

/** The status pills at the bottom of the sidebar. */
export const useCaptureStatus = (): CaptureStatus => {
    const posture = useAIPosture();
    return { capturing: false, elapsed: '', localOnly: posture === 'localOnly', posture };
};
