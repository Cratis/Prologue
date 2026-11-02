// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Agent, AIProvider } from '@cratis/ai';
import { Button, Chip } from '../../Components';
import { ModelTier } from '../Providers/ModelTier';
import { poolPosture } from '../networkPosture';
import { PoolFallback } from '../Pools/PoolFallback';
import { PoolOrdering } from '../Pools/PoolOrdering';
import { PoolView } from '../useAISetup';
import { PoolCeilingControl } from './PoolCeilingControl';
import { arrangementExplanation } from './explanations';
import { fallbackOptions, orderingOptions, postureChips, tierOptions } from './poolLabels';
import { usePoolActions } from './usePoolActions';

export interface PoolCardProps {
    readonly view: PoolView;
    readonly providers: readonly AIProvider[];
    readonly agents: readonly Agent[];
    readonly isInside: (providerId: string) => boolean;
    readonly actions: ReturnType<typeof usePoolActions>;
}

/** One pool: its tier, how it picks, its members in order with their capacity ceiling, and who draws on it. */
export const PoolCard = ({ view, providers, agents, isInside, actions }: PoolCardProps) => {
    const [adding, setAdding] = useState('');
    const [confirmingRemove, setConfirmingRemove] = useState(false);
    const providerById = new Map(providers.map(provider => [provider.id.toString(), provider]));
    const available = providers.filter(provider => !view.members.includes(provider.id.toString()));
    const posture = poolPosture(
        { members: view.members, inPriorityOrder: view.ordering === PoolOrdering.inPriorityOrder, pausesBeforeFallback: view.fallback === PoolFallback.pauseAndAsk },
        isInside);
    const chip = posture === 'empty' ? undefined : postureChips[posture];
    const usedBy = agents.filter(agent => agent.poolId?.toString() === view.pool.id.toString()).map(agent => agent.name);
    const prioritized = view.ordering === PoolOrdering.inPriorityOrder;

    return (
        <div className="poolcard">
            <header>
                <b>{view.pool.name}</b>
                {chip && <Chip variant={chip.variant}>{chip.label}</Chip>}
            </header>
            <div className="row">
                <select aria-label={`Tier of ${view.pool.name}`} value={view.arrangement?.tier ?? ModelTier.balanced} onChange={event => void actions.arrange(view, { tier: Number(event.target.value) })}>
                    {tierOptions.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
                <select aria-label={`Ordering of ${view.pool.name}`} value={view.ordering} onChange={event => void actions.arrange(view, { ordering: Number(event.target.value) })}>
                    {orderingOptions.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
                <select aria-label={`Fallback of ${view.pool.name}`} value={view.fallback} onChange={event => void actions.arrange(view, { fallback: Number(event.target.value) })}>
                    {fallbackOptions.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
            </div>
            {view.members.length === 0 && <span className="muted note">No providers in this pool yet.</span>}
            {view.members.map((id, index) => {
                const provider = providerById.get(id);
                const name = provider?.name ?? 'Unknown provider';
                return (
                    <div key={id} className="member">
                        <span className="ord">{index + 1}</span>
                        <span>
                            {name} <span className="muted small-text">{isInside(id) ? 'in your network' : 'outside'}</span>
                            {provider?.usageCapacity ? <span className="muted small-text"> · ceiling {provider.usageCapacity.toLocaleString('en-US')} tokens</span> : null}
                        </span>
                        <span className="mv">
                            {prioritized && <Button small aria-label={`Move ${name} up`} disabled={index === 0 || actions.isBusy(`arrange-${view.pool.id}`)} onClick={() => void actions.move(view, id, -1)}>↑</Button>}
                            {prioritized && <Button small aria-label={`Move ${name} down`} disabled={index === view.members.length - 1 || actions.isBusy(`arrange-${view.pool.id}`)} onClick={() => void actions.move(view, id, 1)}>↓</Button>}
                            <Button small aria-label={`Remove ${name} from the pool`} disabled={actions.isBusy(`remove-member-${view.pool.id}-${id}`)} onClick={() => void actions.removeMember(view, id)}>×</Button>
                        </span>
                    </div>
                );
            })}
            {available.length > 0 && (
                <select aria-label={`Add a provider to ${view.pool.name}`} value={adding} onChange={event => {
                    const provider = available.find(candidate => candidate.id.toString() === event.target.value);
                    setAdding('');
                    if (provider) void actions.addMember(view, provider);
                }}>
                    <option value="">Add a provider…</option>
                    {available.map(provider => <option key={provider.id.toString()} value={provider.id.toString()}>{provider.name}</option>)}
                </select>
            )}
            <PoolCeilingControl view={view} actions={actions} />
            <span className="muted small-text">Used by {usedBy.length > 0 ? usedBy.join(', ') : 'no agent'}</span>
            <span className="muted small-text">{arrangementExplanation}</span>
            <div className="row">
                {confirmingRemove ? (
                    <>
                        <span className="muted small-text">Agents drawing from this pool stop running until pointed elsewhere.</span>
                        <Button small variant="primary" onClick={() => void actions.remove(view)}>Remove pool</Button>
                        <Button small variant="ghost" onClick={() => setConfirmingRemove(false)}>Keep</Button>
                    </>
                ) : <Button small variant="ghost" onClick={() => setConfirmingRemove(true)}>Remove pool</Button>}
            </div>
        </div>
    );
};
