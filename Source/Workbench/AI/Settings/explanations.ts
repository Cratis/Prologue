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
    'and either continues on the next provider or pauses and asks when its first choice cannot take the work. The Workbench enforces this whenever an agent runs.';

/** Said on the pool arrangement, which the dispatcher follows whenever an agent draws on the pool. */
export const arrangementExplanation =
    'Work goes to the members in this order. In priority order uses the order above; most capacity left first asks each provider what it has left and keeps the order above when it cannot tell. ' +
    'When a member cannot take the work the pool either continues on the next provider or, set to pause and ask, never moves from a provider inside your network to one outside it: the call stops and says so.';

/** Said on the pool ceiling, which the dispatcher holds the pool to. */
export const ceilingExplanation =
    'The most tokens the pool may use in a UTC day, counted from the usage each answer reports. Once it is reached the pool\'s agents stop with a clear message until midnight UTC or until you raise it; the count can run slightly over while calls are in flight.';
