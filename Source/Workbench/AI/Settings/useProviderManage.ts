// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { AIProvider, RemoveAIProvider, SetAIProviderConcurrency, SetAIProviderUsageCapacity } from '@cratis/ai';
import { MarkProviderAsInsideNetwork, MarkProviderAsOutsideNetwork } from '../Network/MarkingProviders/MarkingProviders';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { vendorOf } from './vendors';

/**
 * What the manage panel of one provider does: change its concurrency and usage capacity, say whether it runs inside the
 * network, disconnect a sign-in and remove it.
 * @param provider The provider.
 * @returns The drafts, the actions and which of them are running.
 */
export const useProviderManage = (provider: AIProvider) => {
    const { run, isBusy } = useCommandRunner();
    const [concurrency, setConcurrency] = useState(provider.maxConcurrentJobs ?? 0);
    const [capacity, setCapacity] = useState(provider.usageCapacity ?? 0);
    const [confirmingRemove, setConfirmingRemove] = useState(false);

    const key = (name: string) => `${name}-${provider.id}`;

    const saveConcurrency = () => {
        const command = new SetAIProviderConcurrency();
        command.provider = provider.id;
        command.maxConcurrentJobs = concurrency;
        return run(key('concurrency'), command, `${provider.name}: concurrency saved`);
    };

    const saveCapacity = () => {
        const command = new SetAIProviderUsageCapacity();
        command.provider = provider.id;
        command.usageCapacity = capacity;
        return run(key('capacity'), command, `${provider.name}: usage capacity saved`);
    };

    const markInside = (inside: boolean) => {
        const command = inside ? new MarkProviderAsInsideNetwork() : new MarkProviderAsOutsideNetwork();
        command.provider = provider.id;
        return run(key('network'), command, `${provider.name} is now marked as ${inside ? 'inside' : 'outside'} your network`);
    };

    const disconnect = () => {
        const signIn = vendorOf(provider.type)?.deviceSignIn;
        if (!signIn) return Promise.resolve(false);
        const command = new signIn.disconnect();
        command.provider = provider.id;
        return run(key('disconnect'), command, `${provider.name} disconnected`);
    };

    const remove = () => {
        const command = new RemoveAIProvider();
        command.provider = provider.id;
        return run(key('remove'), command, `${provider.name} removed`);
    };

    return {
        concurrency, setConcurrency, capacity, setCapacity, confirmingRemove, setConfirmingRemove,
        saveConcurrency, saveCapacity, markInside, disconnect, remove,
        isBusy: (name: string) => isBusy(key(name))
    };
};
