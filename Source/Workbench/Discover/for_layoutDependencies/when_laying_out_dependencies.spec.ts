// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { DependencyKind } from '../DependencyKind';
import { layoutDependencies } from '../layoutDependencies';

const dependencies = [
    { from: 'Lending', target: 'Notifications', kind: DependencyKind.http, detail: 'POST /notify' },
    { from: 'Lending', target: 'library-db', kind: DependencyKind.database, detail: 'writes loans' },
    { from: 'Stock-take', target: 'library-db', kind: DependencyKind.database, detail: 'writes copies' },
    { from: 'Stock-take', target: 'Nightly 02:00', kind: DependencyKind.job, detail: 'schedule' }
];

describe('when laying out dependencies', () => {
    const layout = layoutDependencies(['Lending', 'Stock-take', 'Notifications'], dependencies);

    it('puts the systems that depend on something on the top row', () => expect(layout.nodes.filter(node => node.kind === 'system').map(node => node.label).sort()).toEqual(['Lending', 'Notifications', 'Stock-take']));
    it('draws a target that is an analyzed system as that system', () => expect(layout.nodes.filter(node => node.label === 'Notifications')).toHaveLength(1));
    it('draws what is not a system on the bottom row', () => expect(layout.nodes.filter(node => node.y > 100).map(node => node.label).sort()).toEqual(['Nightly 02:00', 'library-db']));
    it('draws a shared target once', () => expect(layout.nodes.filter(node => node.label === 'library-db')).toHaveLength(1));
    it('kinds the bottom nodes by how they are depended on', () => expect(layout.nodes.find(node => node.label === 'library-db')?.kind).toBe('database'));
    it('draws one arrow per dependency', () => expect(layout.edges).toHaveLength(4));
    it('draws an arrow between two systems along the top row', () => {
        const edge = layout.edges.find(candidate => candidate.label === 'POST /notify');
        expect(edge?.y1).toBe(edge?.y2);
    });
    it('keeps every node inside the drawing', () => expect(layout.nodes.every(node => node.x >= 0 && node.x + node.width <= layout.width && node.y + node.height <= layout.height)).toBe(true));
    it('draws nothing for no dependencies', () => expect(layoutDependencies(['Lending'], [])).toMatchObject({ nodes: [], edges: [] }));
});
