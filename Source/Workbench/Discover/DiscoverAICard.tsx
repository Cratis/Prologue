// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Link } from '../Shell/Router';
import { Chip, Panel } from '../Components';
import { Posture, poolPosture } from '../AI/networkPosture';
import { PoolFallback } from '../AI/Pools/PoolFallback';
import { PoolOrdering } from '../AI/Pools/PoolOrdering';
import { postureChips } from '../AI/Settings/poolLabels';
import { useAISetup } from '../AI/useAISetup';
import { useAIPosture } from '../AI/useAIPosture';
import { prologueAgents } from '../AI/Settings/agents';

const codeSentences: Readonly<Record<Exclude<Posture, 'none'>, string>> = {
    localOnly: 'Code is read only by AI providers inside your network.',
    localFirst: 'Code is read by local AI first, and you are asked before any provider outside your network reads it.',
    leavesNetwork: 'Code may be sent to AI providers outside your network.'
};

const codeAgents = prologueAgents.filter(agent => ['prologue-dramaturg', 'prologue-prompter', 'prologue-usher'].includes(agent.purpose));

/** The pool each code-analysis agent draws from and how far its work leaves the network. Providers and pools are set up under Systems. */
export const DiscoverAICard = () => {
    const setup = useAISetup();
    const posture = useAIPosture();

    return (
        <Panel title="AI for code analysis" aside="Cratis.AI">
            <div className="grid stack">
                {codeAgents.map(info => {
                    const agent = setup.agents.find(candidate => candidate.purpose === info.purpose);
                    const view = setup.pools.find(candidate => candidate.pool.id.toString() === agent?.poolId?.toString());
                    const pool = view && poolPosture(
                        { members: view.members, inPriorityOrder: view.ordering === PoolOrdering.inPriorityOrder, pausesBeforeFallback: view.fallback === PoolFallback.pauseAndAsk },
                        setup.isInside);
                    const chip = pool && pool !== 'empty' ? postureChips[pool] : undefined;

                    return (
                        <div key={info.purpose} className="agent-line">
                            <span className="av" aria-hidden="true">{info.mark}</span>
                            <span>
                                <b>{info.name}</b>
                                <span className="muted small-text block">{view ? `${view.pool.name} pool` : setup.loaded ? 'No pool chosen yet' : 'Loading'}</span>
                            </span>
                            {chip && <Chip variant={chip.variant}>{chip.label}</Chip>}
                        </div>
                    );
                })}
            </div>
            {posture !== 'none' && <p className="small-text note">{codeSentences[posture]}</p>}
            <p className="muted small-text note">
                Without a pool the map holds what the routes show; the agents add decision points and dependencies. Providers, pools and agents are set up under <Link to="/systems">Systems → AI</Link>.
            </p>
        </Panel>
    );
};
