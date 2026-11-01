// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModelTier } from '../../ModelTier';
import { PoolFallback } from '../Pools/PoolFallback';
import { PoolOrdering } from '../Pools/PoolOrdering';
import { PoolPosture } from '../networkPosture';

/** The tiers a pool or agent can be for, with the words a person sees. */
export const tierOptions: readonly { readonly value: ModelTier; readonly label: string }[] = [
    { value: ModelTier.fast, label: 'Fast' },
    { value: ModelTier.balanced, label: 'Balanced' },
    { value: ModelTier.powerful, label: 'Powerful' },
    { value: ModelTier.premier, label: 'Premier' }
];

/** How a pool picks among its members. */
export const orderingOptions: readonly { readonly value: PoolOrdering; readonly label: string }[] = [
    { value: PoolOrdering.byRemainingCapacity, label: 'Most capacity left first' },
    { value: PoolOrdering.inPriorityOrder, label: 'In priority order' }
];

/** What a pool does when its first choice cannot take the work. */
export const fallbackOptions: readonly { readonly value: PoolFallback; readonly label: string }[] = [
    { value: PoolFallback.continueOnNext, label: 'Continue on the next provider' },
    { value: PoolFallback.pauseAndAsk, label: 'Pause and ask' }
];

/** The chip a pool's posture shows. */
export const postureChips: Readonly<Record<Exclude<PoolPosture, 'empty'>, { readonly variant: 'ok' | 'warn' | 'bad'; readonly label: string }>> = {
    localOnly: { variant: 'ok', label: 'Stays in your network' },
    localFirst: { variant: 'warn', label: 'Asks before leaving your network' },
    leavesNetwork: { variant: 'bad', label: 'May leave your network' }
};
