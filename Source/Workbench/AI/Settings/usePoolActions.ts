// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AddProviderToPool, AIProvider, CreateAIProviderPool, RemoveAIProviderPool, RemoveProviderFromPool } from '@cratis/ai';
import { Guid } from '@cratis/fundamentals';
import { ModelTier } from '../Providers/ModelTier';
import { ArrangePool } from '../Pools/Arranging/ArrangePool';
import { moveMember } from '../poolOrder';
import { useAICommands } from '../useAICommands';
import { PoolView } from '../useAISetup';

/**
 * The actions on provider pools. Every change to how a pool picks its members is an arrangement the Workbench records,
 * built from the pool as it stands so changing one thing keeps the rest.
 * @returns The actions and which of them are running.
 */
export const usePoolActions = () => {
    const { run, isBusy } = useAICommands();

    const arrange = (view: PoolView, change: Partial<{ tier: ModelTier; ordering: number; fallback: number; order: readonly string[] }>) => {
        const command = new ArrangePool();
        command.pool = view.pool.id;
        command.tier = change.tier ?? view.arrangement?.tier ?? ModelTier.balanced;
        command.ordering = change.ordering ?? view.ordering;
        command.fallback = change.fallback ?? view.fallback;
        command.order = (change.order ?? view.members).map(id => Guid.parse(id));
        return run(`arrange-${view.pool.id}`, command);
    };

    const move = (view: PoolView, providerId: string, offset: -1 | 1) => arrange(view, { order: moveMember(view.members, providerId, offset) });

    const addMember = (view: PoolView, provider: AIProvider) => {
        const command = new AddProviderToPool();
        command.pool = view.pool.id;
        command.provider = provider.id;
        return run(`add-member-${view.pool.id}`, command);
    };

    const removeMember = (view: PoolView, providerId: string) => {
        const command = new RemoveProviderFromPool();
        command.pool = view.pool.id;
        command.provider = Guid.parse(providerId);
        return run(`remove-member-${view.pool.id}-${providerId}`, command);
    };

    const create = (name: string) => {
        const command = new CreateAIProviderPool();
        command.name = name.trim();
        return run('create-pool', command, `Created ${name.trim()}`);
    };

    const remove = (view: PoolView) => {
        const command = new RemoveAIProviderPool();
        command.pool = view.pool.id;
        return run(`remove-pool-${view.pool.id}`, command, `Removed ${view.pool.name}`);
    };

    return { arrange, move, addMember, removeMember, create, remove, isBusy };
};
