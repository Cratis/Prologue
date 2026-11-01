// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, Chip } from '../../Components';
import { postureLabels } from '../networkPosture';
import { AISetup } from '../useAISetup';
import { postureOf } from '../useAIPosture';
import { poolsExplanation } from './explanations';
import { PoolCard } from './PoolCard';
import { usePoolActions } from './usePoolActions';

export interface PoolsSectionProps {
    readonly setup: AISetup;
}

/** The pools and the form that creates one, with the posture they add up to. */
export const PoolsSection = ({ setup }: PoolsSectionProps) => {
    const actions = usePoolActions();
    const posture = postureOf(setup);
    const [name, setName] = useState('');

    const create = async () => {
        if (await actions.create(name)) setName('');
    };

    return (
        <div>
            <div className="ai-sub">Pools{posture !== 'none' && <Chip variant={posture === 'leavesNetwork' ? 'bad' : posture === 'localFirst' ? 'warn' : 'ok'}>{postureLabels[posture]}</Chip>}</div>
            <p className="muted note">{poolsExplanation}</p>
            <div className="grid stack">
                {setup.loaded && setup.pools.length === 0 && <span className="muted note">No pools yet. Create one, add providers to it and point an agent at it.</span>}
                {setup.pools.map(view => (
                    <PoolCard key={view.pool.id.toString()} view={view} providers={setup.providers} agents={setup.agents} isInside={setup.isInside} actions={actions} />
                ))}
                <div className="row">
                    <input aria-label="Name of the new pool" value={name} placeholder="A recognizable name for the pool" onChange={event => setName(event.target.value)} />
                    <Button small disabled={name.trim().length === 0 || actions.isBusy('create-pool')} onClick={() => void create()}>Create pool</Button>
                </div>
            </div>
        </div>
    );
};
