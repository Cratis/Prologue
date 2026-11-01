// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { PendingSubscriptionSignIns } from '@cratis/ai';
import { Button } from '../../Components';
import { AISetup } from '../useAISetup';
import { AddProviderForm } from './AddProviderForm';
import { ProviderRow } from './ProviderRow';

export interface ProvidersSectionProps {
    readonly setup: AISetup;
}

/** The providers list and the form that adds one. */
export const ProvidersSection = ({ setup }: ProvidersSectionProps) => {
    const [adding, setAdding] = useState(false);
    const [pending] = PendingSubscriptionSignIns.use();
    const pendingById = new Map(pending.data.map(signIn => [signIn.id.toString(), signIn]));

    return (
        <div>
            <div className="ai-sub">Providers<Button small onClick={() => setAdding(open => !open)}>Add provider</Button></div>
            {setup.loaded && setup.providers.length === 0 && !adding && (
                <p className="muted note">No providers yet. Add one, such as a model in your network, and put it in a pool.</p>
            )}
            {setup.providers.map(provider => (
                <ProviderRow
                    key={provider.id.toString()}
                    provider={provider}
                    inside={setup.isInside(provider.id.toString())}
                    pendingSignIn={pendingById.get(provider.id.toString())} />
            ))}
            {adding && <AddProviderForm onClose={() => setAdding(false)} />}
        </div>
    );
};
