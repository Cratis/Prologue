// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useRef, useState } from 'react';
import { useToast } from '../Components';

/** What the panel needs from a command: that it can be executed and says how that went. */
export interface Runnable {
    execute: () => Promise<{ isSuccess: boolean; validationResults: { message: string }[]; exceptionMessages: string[] }>;
}

/**
 * Runs the commands of the AI panel behind an in-flight guard: a control that is busy cannot fire again, and a command
 * that fails says why instead of failing silently.
 * @returns `run` to execute a command under a key, and `isBusy` to disable the control that fired it.
 */
export const useAICommands = () => {
    const toast = useToast();
    const running = useRef(new Set<string>());
    const [busy, setBusy] = useState<ReadonlySet<string>>(new Set());

    const run = useCallback(async (key: string, command: Runnable, success?: string): Promise<boolean> => {
        if (running.current.has(key)) return false;
        running.current.add(key);
        setBusy(new Set(running.current));
        try {
            const result = await command.execute();
            if (result.isSuccess) {
                if (success) toast.show(success);
                return true;
            }
            const reasons = [...result.validationResults.map(validation => validation.message), ...result.exceptionMessages];
            toast.show(reasons[0] ?? 'That did not work');
            return false;
        } finally {
            running.current.delete(key);
            setBusy(new Set(running.current));
        }
    }, [toast]);

    return { run, isBusy: (key: string) => busy.has(key) };
};
