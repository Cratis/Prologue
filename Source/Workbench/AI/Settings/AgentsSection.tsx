// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Harness } from '@cratis/ai';
import { Chip } from '../../Components';
import { AISetup } from '../useAISetup';
import { prologueAgents } from './agents';
import { useAgentActions } from './useAgentActions';

export interface AgentsSectionProps {
    readonly setup: AISetup;
}

const harnessOptions: readonly { readonly value: Harness; readonly label: string }[] = [
    { value: Harness.claude, label: 'Claude' },
    { value: Harness.pi, label: 'Pi' },
    { value: Harness.copilot, label: 'Copilot' }
];

/** Prologue's agents, each with the pool it draws from and, for the ones that run as jobs, the harness. */
export const AgentsSection = ({ setup }: AgentsSectionProps) => {
    const actions = useAgentActions();

    return (
        <div>
            <div className="ai-sub">Agents</div>
            <div className="grid stack">
                {prologueAgents.map(info => {
                    const agent = setup.agents.find(candidate => candidate.purpose === info.purpose);
                    const busy = actions.isBusy(info);
                    return (
                        <div key={info.purpose} className="poolcard">
                            <div className="agent">
                                <span className="av" aria-hidden="true">{info.mark}</span>
                                <span><b>{info.name}</b> <Chip>{info.mode === 'job' ? 'Job · worker container' : 'Chat'}</Chip></span>
                            </div>
                            <span className="muted small-text">{info.summary}</span>
                            <div className="row">
                                <select aria-label={`Pool for ${info.name}`} disabled={busy || setup.pools.length === 0} value={agent?.poolId?.toString() ?? ''}
                                    onChange={event => void actions.configure(info, agent, { poolId: event.target.value })}>
                                    <option value="" disabled>{setup.pools.length === 0 ? 'Create a pool first' : 'Choose a pool'}</option>
                                    {setup.pools.map(view => <option key={view.pool.id.toString()} value={view.pool.id.toString()}>{view.pool.name}</option>)}
                                </select>
                                {info.mode === 'job' && (
                                    <select aria-label={`Harness for ${info.name}`} disabled={busy} value={agent?.harness ?? Harness.claude}
                                        onChange={event => void actions.configure(info, agent, { harness: Number(event.target.value) as Harness })}>
                                        {harnessOptions.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}
                                    </select>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};
