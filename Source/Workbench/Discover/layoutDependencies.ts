// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DependencyKind } from './DependencyKind';

export interface DependencyInput {
    readonly from: string;
    readonly target: string;
    readonly kind: DependencyKind;
    readonly detail: string;
}

export type NodeKind = 'system' | 'service' | 'database' | 'job' | 'queue';

export interface LaidOutNode {
    readonly id: string;
    readonly label: string;
    readonly kind: NodeKind;
    readonly x: number;
    readonly y: number;
    readonly width: number;
    readonly height: number;
}

export interface LaidOutEdge {
    readonly key: string;
    readonly kind: DependencyKind;
    readonly label: string;
    readonly x1: number;
    readonly y1: number;
    readonly x2: number;
    readonly y2: number;
}

export interface DependencyLayout {
    readonly width: number;
    readonly height: number;
    readonly nodes: readonly LaidOutNode[];
    readonly edges: readonly LaidOutEdge[];
}

const width = 360;
const nodeHeight = 30;
const topY = 18;
const bottomY = 150;
const margin = 10;

const nodeKind = (kind: DependencyKind): NodeKind => {
    switch (kind) {
        case DependencyKind.database: return 'database';
        case DependencyKind.job: return 'job';
        case DependencyKind.queue: return 'queue';
        default: return 'service';
    }
};

const spread = (count: number, index: number, nodeWidth: number) => {
    const slot = (width - margin * 2) / count;
    return margin + slot * index + (slot - nodeWidth) / 2;
};

/**
 * Lays dependencies out as a diagram: the analyzed systems on top and what they depend on below. A target that is itself an
 * analyzed system joins the top row, so a call between two systems is an arrow along it.
 * @param systems The names of the analyzed systems.
 * @param dependencies The dependencies.
 * @returns Where each node and arrow goes in a 360 wide drawing.
 */
export const layoutDependencies = (systems: readonly string[], dependencies: readonly DependencyInput[]): DependencyLayout => {
    const systemIds = new Map(systems.map(name => [name.toLowerCase(), name]));
    const topNames = [...systemIds.values()].filter(name => dependencies.some(dependency => dependency.from === name || systemIds.get(dependency.target.toLowerCase()) === name));
    const bottom = new Map<string, { label: string; kind: NodeKind }>();

    for (const dependency of dependencies) {
        const asSystem = systemIds.get(dependency.target.toLowerCase());
        if (!asSystem && !bottom.has(dependency.target.toLowerCase())) bottom.set(dependency.target.toLowerCase(), { label: dependency.target, kind: nodeKind(dependency.kind) });
    }

    const nodeWidth = (count: number) => Math.min(120, Math.floor((width - margin * 2) / Math.max(count, 1)) - 8);
    const topWidth = nodeWidth(topNames.length);
    const bottomWidth = nodeWidth(bottom.size);

    const nodes: LaidOutNode[] = [
        ...topNames.map<LaidOutNode>((name, index) => ({ id: `system:${name.toLowerCase()}`, label: name, kind: 'system', x: spread(topNames.length, index, topWidth), y: topY, width: topWidth, height: nodeHeight })),
        ...[...bottom.entries()].map<LaidOutNode>(([key, node], index) => ({ id: `target:${key}`, label: node.label, kind: node.kind, x: spread(bottom.size, index, bottomWidth), y: bottomY, width: bottomWidth, height: nodeHeight }))
    ];

    const byId = new Map(nodes.map(node => [node.id, node]));
    const edges: LaidOutEdge[] = [];
    for (const dependency of dependencies) {
        const from = byId.get(`system:${dependency.from.toLowerCase()}`);
        const asSystem = systemIds.get(dependency.target.toLowerCase());
        const to = asSystem ? byId.get(`system:${asSystem.toLowerCase()}`) : byId.get(`target:${dependency.target.toLowerCase()}`);
        if (!from || !to || from === to) continue;

        const sameRow = from.y === to.y;
        edges.push({
            key: `${from.id}>${to.id}:${dependency.kind}`,
            kind: dependency.kind,
            label: dependency.detail,
            x1: sameRow ? (to.x > from.x ? from.x + from.width : from.x) : from.x + from.width / 2,
            y1: sameRow ? from.y + from.height / 2 : from.y + from.height,
            x2: sameRow ? (to.x > from.x ? to.x : to.x + to.width) : to.x + to.width / 2,
            y2: sameRow ? to.y + to.height / 2 : to.y
        });
    }

    return { width, height: bottom.size > 0 ? bottomY + nodeHeight + topY : topY * 2 + nodeHeight, nodes, edges };
};
