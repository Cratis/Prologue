// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { FormEvent, useState } from 'react';
import { AIProviderType } from '@cratis/ai';
import { useAICommands } from '../useAICommands';
import { Vendor, vendorOf, vendors } from './vendors';

interface Draft {
    readonly type: AIProviderType;
    readonly name: string;
    readonly endpoint: string;
    readonly apiKey: string;
    readonly maxConcurrentJobs: number;
}

const draftFor = (vendor: Vendor): Draft => ({
    type: vendor.type,
    name: '',
    endpoint: vendor.endpoint?.defaultValue ?? '',
    apiKey: '',
    maxConcurrentJobs: 2
});

/**
 * The state of the add provider form: the vendor chosen, what was typed for it and the add itself. The API key lives
 * only in this draft until the command is sent, and is cleared whether the add worked or not.
 * @param onAdded Called once a provider was added.
 * @returns The vendor, the draft, how to change them, and the submit.
 */
export const useAddProviderForm = (onAdded: () => void) => {
    const { run, isBusy } = useAICommands();
    const [draft, setDraft] = useState(() => draftFor(vendors[0]));
    const vendor = vendorOf(draft.type) ?? vendors[0];

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const command = new vendor.add();
        command.name = draft.name.trim();
        if (vendor.endpoint) command.endpoint = draft.endpoint.trim();
        // The proxy marks the key as required; an empty key (a model in the network that needs none) is a value, undefined is not.
        if (vendor.apiKey) command.apiKey = draft.apiKey;
        command.maxConcurrentJobs = draft.maxConcurrentJobs;
        const added = await run('add-provider', command, `${draft.name.trim()} added. Add it to a pool to use it.`);
        setDraft(previous => ({ ...previous, apiKey: '' }));
        if (added) onAdded();
    };

    return {
        vendor,
        draft,
        adding: isBusy('add-provider'),
        canAdd: draft.name.trim().length > 0 && (!vendor.endpoint || draft.endpoint.trim().length > 0),
        chooseVendor: (type: AIProviderType) => setDraft(draftFor(vendorOf(type) ?? vendors[0])),
        change: (values: Partial<Draft>) => setDraft(previous => ({ ...previous, ...values })),
        submit
    };
};
