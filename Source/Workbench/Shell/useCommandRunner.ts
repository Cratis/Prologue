// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Command } from '@cratis/arc/commands';
import { useCallback, useRef, useState } from 'react';
import { useToast } from '../Components';

/** The outcome message a command result carries when it did not succeed: the refusal, the exception or the authorization failure. */
export const failureMessage = (result: {
    readonly validationResults: readonly { readonly message: string }[];
    readonly exceptionMessages: readonly string[];
    readonly authorizationFailureReason: string;
}): string =>
    result.validationResults[0]?.message || result.exceptionMessages[0] || result.authorizationFailureReason || 'The command did not succeed';

/**
 * Runs commands for a component. A control that fires a command passes a `key`; while a command with that key runs, `isBusy(key)`
 * is true so the control can be disabled, and a second run with the same key is ignored. A refusal or failure is shown as a toast.
 * @returns `run` to execute a command and `isBusy` to ask whether a keyed command is running.
 */
export const useCommandRunner = () => {
    const toast = useToast();
    const inFlight = useRef(new Set<string>());
    const [, setTick] = useState(0);

    const run = useCallback(async (key: string, command: Command, success?: string): Promise<boolean> => {
        if (inFlight.current.has(key)) return false;
        inFlight.current.add(key);
        setTick(tick => tick + 1);

        try {
            const result = await command.execute();
            if (result.isSuccess) {
                if (success) toast.show(success);
                return true;
            }
            toast.show(failureMessage(result));
            return false;
        } finally {
            inFlight.current.delete(key);
            setTick(tick => tick + 1);
        }
    }, [toast]);

    const isBusy = useCallback((key: string) => inFlight.current.has(key), []);

    return { run, isBusy };
};
