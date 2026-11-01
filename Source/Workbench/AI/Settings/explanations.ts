// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** Said wherever a provider's concurrency is set. */
export const concurrencyExplanation =
    'How many units of work may run on this provider at the same time. Zero means no limit. ' +
    'Work that arrives while the provider is full stays queued and starts as soon as capacity frees up.';

/** Said wherever a provider's usage capacity is set. */
export const usageCapacityExplanation =
    'How many tokens this provider may consume within its usage-reporting window. Zero means no ceiling. ' +
    'Set one to let pool selection rank this provider by remaining capacity.';

/** Said where a provider is marked as inside or outside the network. */
export const networkExplanation =
    'Providers inside your network keep captures and code on your side. A model served by Ollama, vLLM or LM Studio at a private address is marked inside for you; ' +
    'anything else counts as outside until you say otherwise.';

/** Said on the pools list, where the order and the fallback decide whether work can leave the network. */
export const poolsExplanation =
    'A named group of providers an agent draws from. A pool tries its members by most capacity left, or in the order you put them, ' +
    'and either continues on the next provider or pauses and asks when its first choice cannot take the work.';

/** Said on the pool arrangement, which the Workbench keeps itself. */
export const arrangementExplanation =
    'The tier, order, ordering and fallback are recorded by the Workbench and are what the network posture is derived from.';
