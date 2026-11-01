// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode, useCallback, useMemo, useState } from 'react';
import { defaultOptionalOn, defaultPrologues, OptionalStepState, ShellState, ShellStateContext } from './shellState';
import { StepId } from './steps';

/** Provides the shell state from memory until the backend supplies it. */
export const ShellStateProvider = ({ children }: { children: ReactNode }) => {
    const [selectedId, setSelectedId] = useState(defaultPrologues[0].id);
    const [optionalOn, setOptionalOn] = useState<OptionalStepState>(defaultOptionalOn);

    const setOptionalStep = useCallback((id: StepId, on: boolean) => setOptionalOn(previous => ({ ...previous, [id]: on })), []);

    const value = useMemo<ShellState>(() => ({
        prologues: defaultPrologues,
        selectedId,
        selectPrologue: setSelectedId,
        optionalOn,
        setOptionalStep
    }), [selectedId, optionalOn, setOptionalStep]);

    return <ShellStateContext.Provider value={value}>{children}</ShellStateContext.Provider>;
};
