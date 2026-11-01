// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, useContext } from 'react';
import { StepId } from './steps';

/** A prologue as the switcher shows it. */
export interface PrologueSummary {
    readonly id: string;
    readonly name: string;
    readonly system: string;
}

/** The optional steps that can be switched off, by step id. */
export type OptionalStepState = Readonly<Partial<Record<StepId, boolean>>>;

export interface ShellState {
    readonly prologues: readonly PrologueSummary[];
    readonly selectedId: string;
    readonly selectPrologue: (id: string) => void;
    readonly optionalOn: OptionalStepState;
    readonly setOptionalStep: (id: StepId, on: boolean) => void;
}

/**
 * Temporary in-memory defaults for the shell. Wave 2 replaces the provider that fills these (the prologue list and
 * which optional steps are on) with Arc queries and commands; the hooks below keep their signatures.
 */
export const defaultPrologues: readonly PrologueSummary[] = [
    { id: 'lending-baseline', name: 'Lending baseline', system: 'Library system' }
];

export const defaultOptionalOn: OptionalStepState = { discover: true, capture: true, rebuild: true };

export const ShellStateContext = createContext<ShellState>({
    prologues: defaultPrologues,
    selectedId: defaultPrologues[0].id,
    selectPrologue: () => undefined,
    optionalOn: defaultOptionalOn,
    setOptionalStep: () => undefined
});

/** The prologues to choose from and the selected one; feeds the sidebar switcher and every page eyebrow. */
export const usePrologues = () => {
    const { prologues, selectedId, selectPrologue } = useContext(ShellStateContext);
    const current = prologues.find(prologue => prologue.id === selectedId) ?? prologues[0];
    return { prologues, current, selectPrologue };
};

/** Whether optional steps are on for the selected prologue, and the means to change that. */
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
}

/** The status pills at the bottom of the sidebar. */
export const useCaptureStatus = (): CaptureStatus => ({ capturing: false, elapsed: '', localOnly: true });
