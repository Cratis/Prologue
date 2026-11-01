// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AIProvider, CapacityFor } from '@cratis/ai';

export interface ProviderCapacityProps {
    readonly provider: AIProvider;
}

/** The capacity bar of a provider whose remaining allowance is known; nothing for one that is unmetered or unknown. */
export const ProviderCapacity = ({ provider }: ProviderCapacityProps) => {
    const [result] = CapacityFor.use({ providerId: provider.id });
    const capacity = result.data;
    // A provider with no metering has a capacity document without windows.
    const windows = capacity?.windows ?? [];
    if (!result.hasData || !capacity || windows.length === 0) return null;

    const tightest = windows.reduce((narrowest, window) => window.remaining < narrowest.remaining ? window : narrowest);
    const left = Math.round(capacity.headroom * 100);
    return (
        <>
            <div className="capbar" role="img" aria-label={`${left}% of ${tightest.name} left`}><i style={{ width: `${left}%` }} /></div>
            <small>{left}% left in {tightest.name.toLowerCase()}{tightest.resetsAt ? ` · resets ${tightest.resetsAt.toLocaleDateString()}` : ''}</small>
        </>
    );
};
