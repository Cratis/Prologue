// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The order a pool tries its members in: the people-chosen order first (for the members still in the pool), then any
 * member the order does not mention yet, in the order the pool lists them.
 * @param members The provider ids in the pool, as the pool lists them.
 * @param order The provider ids in the order a person chose, which may mention providers no longer in the pool.
 * @returns Every member once, first choice first.
 */
export const effectiveOrder = (members: readonly string[], order: readonly string[]): string[] => {
    const inPool = new Set(members);
    const chosen = order.filter((id, index) => inPool.has(id) && order.indexOf(id) === index);
    return [...chosen, ...members.filter(id => !chosen.includes(id))];
};

/**
 * Moves one member up or down the order.
 * @param order The order as it stands.
 * @param id The member to move.
 * @param offset -1 to move it up, 1 to move it down.
 * @returns The new order; the same order when the member cannot move that way.
 */
export const moveMember = (order: readonly string[], id: string, offset: -1 | 1): string[] => {
    const from = order.indexOf(id);
    const to = from + offset;
    if (from < 0 || to < 0 || to >= order.length) return [...order];
    const moved = [...order];
    [moved[from], moved[to]] = [moved[to], moved[from]];
    return moved;
};
