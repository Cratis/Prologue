// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AllAgents } from '@cratis/ai';

const interpreterPurpose = 'prologue-interpreter';

/**
 * Whether the Interpreter agent has an AI provider or pool to run on. Cratis.AI lists only agents that were configured, and an
 * empty list reads the same as one that has not answered yet, so nothing is claimed until some agent is listed. Without a
 * provider the server answers a question with the reason, which the page then shows.
 * @returns `loaded` once the setup answered and `configured` when the agent can be asked.
 */
export const useInterpreterReady = (): { readonly loaded: boolean; readonly configured: boolean } => {
    const [agents] = AllAgents.use();
    const interpreter = agents.data.find(agent => agent.purpose === interpreterPurpose);
    const named = (id?: unknown) => id !== undefined && id !== null;
    return { loaded: agents.data.length > 0, configured: named(interpreter?.providerId) || named(interpreter?.poolId) };
};
