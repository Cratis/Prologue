// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useRef, useState } from 'react';
import { useToast } from '../Components';
import { executeCommand, FailedResult } from './commandFailures';

/** What the runner needs from a command: that it can be executed and says how that went. */
export interface Runnable {
    execute: () => Promise<FailedResult & { readonly isSuccess: boolean }>;
}

/**
 * Runs commands for a component, Arc and Cratis.AI commands alike. A control that fires a command passes a `key`; while a
 * command with that key runs, `isBusy(key)` is true so the control can be disabled, and a second run with the same key is
 * ignored. A refusal, a failure or a command that could not be sent at all is shown as a toast.
 * @returns `run` to execute a command and `isBusy` to ask whether a keyed command is running.
 */
export const useCommandRunner = () => {
    const toast = useToast();
    const inFlight = useRef(new Set<string>());
    const [, setTick] = useState(0);

    const run = useCallback(async (key: string, command: Runnable, success?: string): Promise<boolean> => {
        if (inFlight.current.has(key)) return false;
        inFlight.current.add(key);
        setTick(tick => tick + 1);

        try {
            const outcome = await executeCommand(command, success);
            if (outcome.message) toast.show(outcome.message);
            return outcome.succeeded;
        } finally {
            inFlight.current.delete(key);
            setTick(tick => tick + 1);
        }
    }, [toast]);

    const isBusy = useCallback((key: string) => inFlight.current.has(key), []);

    return { run, isBusy };
};
