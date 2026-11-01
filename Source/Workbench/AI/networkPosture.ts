// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** How far the AI work leaves the customer's network, in the words the shell shows. */
export type Posture = 'none' | 'localOnly' | 'localFirst' | 'leavesNetwork';

/** What the posture of one pool needs to know about it. */
export interface PosturePool {
    /** The pool's members' provider ids, in the order the pool tries them. */
    readonly members: readonly string[];

    /** Whether the pool tries its members in the order a person put them in. */
    readonly inPriorityOrder: boolean;

    /** Whether the pool pauses and asks before it falls back to the next provider. */
    readonly pausesBeforeFallback: boolean;
}

/** The posture of one pool; a pool without members has none. */
export type PoolPosture = Exclude<Posture, 'none'> | 'empty';

const privateSuffixes = ['.local', '.lan', '.internal', '.intranet', '.home', '.corp'];

const isPrivateAddress = (host: string): boolean => {
    const parts = host.split('.').map(Number);
    if (parts.length !== 4 || parts.some(part => !Number.isInteger(part) || part < 0 || part > 255)) return false;
    const [first, second] = parts;
    return first === 10
        || first === 127
        || (first === 172 && second >= 16 && second <= 31)
        || (first === 192 && second === 168)
        || (first === 169 && second === 254);
};

/**
 * Whether an endpoint points inside a network rather than at the public internet: localhost, a private or loopback
 * address, a name that ends in a private suffix, or a bare host name that only a local resolver knows.
 * @param endpoint The endpoint a person typed, with or without a scheme.
 * @returns True when the endpoint is private.
 */
export const isPrivateEndpoint = (endpoint: string | undefined): boolean => {
    const text = endpoint?.trim();
    if (!text) return false;
    let host: string;
    try {
        host = new URL(/^[a-z][a-z0-9+.-]*:\/\//i.test(text) ? text : `http://${text}`).hostname.toLowerCase();
    } catch {
        return false;
    }
    host = host.replace(/^\[|\]$/g, '');
    if (host === 'localhost' || host.endsWith('.localhost') || host === '::1') return true;
    if (isPrivateAddress(host)) return true;
    if (privateSuffixes.some(suffix => host.endsWith(suffix))) return true;
    return !host.includes('.') && !host.includes(':');
};

/**
 * Whether a provider runs inside the customer's network. A person's mark wins; with no mark, only an
 * OpenAI-compatible provider with a private endpoint counts as inside, because that is how Ollama, vLLM and LM Studio are reached.
 * @param isOpenAICompatible Whether the provider is an OpenAI-compatible one.
 * @param endpoint The provider's endpoint, when it has one.
 * @param mark What a person said, or undefined when nobody did.
 * @returns True when the provider is inside the network.
 */
export const isInsideNetwork = (isOpenAICompatible: boolean, endpoint: string | undefined, mark: boolean | undefined): boolean =>
    mark ?? (isOpenAICompatible && isPrivateEndpoint(endpoint));

/**
 * The posture of one pool: local only when every member is inside, local first when it starts on an inside provider
 * in the order a person chose and pauses before going outside, and otherwise leaving the network.
 * @param pool The pool.
 * @param isInside Whether each provider, by id, is inside the network.
 * @returns The posture, or empty for a pool without members.
 */
export const poolPosture = (pool: PosturePool, isInside: (providerId: string) => boolean): PoolPosture => {
    if (pool.members.length === 0) return 'empty';
    if (pool.members.every(isInside)) return 'localOnly';
    return pool.inPriorityOrder && isInside(pool.members[0]) && pool.pausesBeforeFallback ? 'localFirst' : 'leavesNetwork';
};

/**
 * The posture across all pools: the weakest posture any pool with members has.
 * @param pools Every pool.
 * @param isInside Whether each provider, by id, is inside the network.
 * @returns The posture, or none when no pool has a member.
 */
export const derivePosture = (pools: readonly PosturePool[], isInside: (providerId: string) => boolean): Posture => {
    const postures = pools.map(pool => poolPosture(pool, isInside)).filter(posture => posture !== 'empty');
    if (postures.length === 0) return 'none';
    if (postures.includes('leavesNetwork')) return 'leavesNetwork';
    return postures.includes('localFirst') ? 'localFirst' : 'localOnly';
};

/** What each posture is called in the shell. */
export const postureLabels: Readonly<Record<Exclude<Posture, 'none'>, string>> = {
    localOnly: 'Local only',
    localFirst: 'Local first',
    leavesNetwork: 'Leaves network'
};
