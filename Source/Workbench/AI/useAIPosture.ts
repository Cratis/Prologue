// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PoolFallback } from './Pools/PoolFallback';
import { PoolOrdering } from './Pools/PoolOrdering';
import { derivePosture, Posture } from './networkPosture';
import { AISetup, useAISetup } from './useAISetup';

/**
 * Derives the posture from an AI setup that was already read.
 * @param setup The AI setup.
 * @returns The posture; none until the pools have answered.
 */
export const postureOf = ({ loaded, pools, isInside }: AISetup): Posture => loaded
    ? derivePosture(pools.map(view => ({
        members: view.members,
        inPriorityOrder: view.ordering === PoolOrdering.inPriorityOrder,
        pausesBeforeFallback: view.fallback === PoolFallback.pauseAndAsk
    })), isInside)
    : 'none';

/**
 * How far the AI work leaves the customer's network: local only when every pool member is inside, local first when
 * each pool starts inside and pauses before going outside, and otherwise leaving the network. It is none until the
 * pools have answered and while no pool has a member, so the shell claims nothing it cannot support.
 * @returns The posture.
 */
export const useAIPosture = (): Posture => postureOf(useAISetup());
