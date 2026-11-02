// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { AIProvider, AIProviderPool, AIProviderType, Agent, AllAIProviderPools, AllAIProviders, AllAgents } from '@cratis/ai';
import { AllPoolCeilings } from './Pools/Ceiling/PoolCeiling';
import { AllPoolSpends } from './Pools/Spending/PoolSpend';
import { usedToday, utcDay } from './poolUsage';
import { AllProviderNetworkPositions } from './Network/ProviderNetworkPosition';
import { AllPoolArrangements, PoolArrangement } from './Pools/PoolArrangement';
import { PoolFallback } from './Pools/PoolFallback';
import { PoolOrdering } from './Pools/PoolOrdering';
import { effectiveOrder } from './poolOrder';
import { isInsideNetwork } from './networkPosture';

/** A pool as the panel and the posture see it: Cratis.AI's pool joined with the Workbench's arrangement of it. */
export interface PoolView {
    readonly pool: AIProviderPool;
    readonly arrangement?: PoolArrangement;
    readonly ordering: PoolOrdering;
    readonly fallback: PoolFallback;

    /** The members' provider ids in the order the pool tries them. */
    readonly members: readonly string[];

    /** The most tokens the pool may use in a day (UTC); zero when it has no ceiling. */
    readonly ceiling: number;

    /** The tokens the pool used today. */
    readonly usedToday: number;
}

/** Everything the AI panel and the shell's posture read: Cratis.AI's providers, pools and agents plus the Workbench's own facts. */
export interface AISetup {
    /** Whether every list has answered at least once; nothing is claimed about an empty list before that. */
    readonly loaded: boolean;
    readonly providers: readonly AIProvider[];
    readonly pools: readonly PoolView[];
    readonly agents: readonly Agent[];

    /** Whether a provider runs inside the customer's network. */
    readonly isInside: (providerId: string) => boolean;

    /** Whether a person marked the provider, rather than the Workbench defaulting it. */
    readonly isMarked: (providerId: string) => boolean;
}

/**
 * Reads the AI setup: Cratis.AI's providers, pools and agents joined with the Workbench's pool arrangements and network marks.
 * @returns The {@link AISetup}.
 */
export const useAISetup = (): AISetup => {
    const [providersResult] = AllAIProviders.use();
    const [poolsResult] = AllAIProviderPools.use();
    const [agentsResult] = AllAgents.use();
    const [arrangementsResult] = AllPoolArrangements.use();
    const [positionsResult] = AllProviderNetworkPositions.use();
    const [ceilingsResult] = AllPoolCeilings.use();
    const [spendsResult] = AllPoolSpends.use();

    const providers = providersResult.data;
    const pools = poolsResult.data;
    const agents = agentsResult.data;
    const arrangements = arrangementsResult.data;
    const positions = positionsResult.data;
    const ceilings = ceilingsResult.data;
    const spends = spendsResult.data;

    return useMemo(() => {
        const marks = new Map(positions.map(position => [position.id, position.isInside]));
        const providerById = new Map(providers.map(provider => [provider.id.toString(), provider]));
        const arrangementById = new Map(arrangements.map(arrangement => [arrangement.id, arrangement]));
        const ceilingById = new Map(ceilings.map(ceiling => [ceiling.id, ceiling.tokensPerDay]));
        const spendById = new Map(spends.map(spend => [spend.id, spend]));
        const today = utcDay(new Date());

        const poolViews = pools.map<PoolView>(pool => {
            const arrangement = arrangementById.get(pool.id.toString());
            const ordering = arrangement?.ordering ?? PoolOrdering.byRemainingCapacity;
            const listed = (pool.members ?? []).map(member => member.providerId.toString());
            const order = (arrangement?.order ?? []).map(id => id.toString());
            return { pool, arrangement, ordering, fallback: arrangement?.fallback ?? PoolFallback.continueOnNext, members: ordering === PoolOrdering.inPriorityOrder ? effectiveOrder(listed, order) : listed, ceiling: ceilingById.get(pool.id.toString()) ?? 0, usedToday: usedToday(spendById.get(pool.id.toString()), today) };
        });

        const isInside = (providerId: string) => {
            const provider = providerById.get(providerId);
            return provider !== undefined && isInsideNetwork(provider.type === AIProviderType.openAICompatible, provider.endpoint, marks.get(providerId));
        };

        return {
            // An empty list has no data, so loaded means every query has answered, not that every query holds something.
            loaded: !providersResult.isPerforming && !poolsResult.isPerforming && !arrangementsResult.isPerforming && !positionsResult.isPerforming,
            providers,
            pools: poolViews,
            agents,
            isInside,
            isMarked: (providerId: string) => marks.has(providerId)
        };
    }, [providers, pools, agents, arrangements, positions, ceilings, spends, providersResult.isPerforming, poolsResult.isPerforming, arrangementsResult.isPerforming, positionsResult.isPerforming]);
};
