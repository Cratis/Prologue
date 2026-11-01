// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Agent, ConfigureAgent, Effort, Harness } from '@cratis/ai';
import { Guid } from '@cratis/fundamentals';
import { ModelTier } from '../Providers/ModelTier';
import { useAICommands } from '../useAICommands';
import { PrologueAgentInfo } from './agents';

/**
 * The actions on an agent. Cratis.AI configures an agent as a whole, so a change is built from the agent as it stands
 * and keeps everything it did not change: tier, effort and the description.
 * @returns The actions and which of them are running.
 */
export const useAgentActions = () => {
    const { run, isBusy } = useAICommands();

    const configure = (info: PrologueAgentInfo, agent: Agent | undefined, change: { poolId?: string; harness?: Harness }) => {
        const command = new ConfigureAgent();
        command.purpose = info.purpose;
        command.name = agent?.name ?? info.name;
        command.description = agent?.description ?? info.summary;
        command.tier = (agent?.tier ?? ModelTier.balanced) as number;
        command.effort = agent?.effort ?? Effort.medium;
        command.harness = change.harness ?? agent?.harness ?? Harness.claude;
        const poolId = change.poolId ?? agent?.poolId?.toString();
        if (poolId) command.poolId = Guid.parse(poolId);
        else command.providerId = agent?.providerId;
        return run(`agent-${info.purpose}`, command, `Saved ${info.name}`);
    };

    return { configure, isBusy: (info: PrologueAgentInfo) => isBusy(`agent-${info.purpose}`) };
};
