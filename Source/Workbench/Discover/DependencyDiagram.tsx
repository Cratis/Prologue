// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { EmptyState, Panel } from '../Components';
import { CodeDependency } from './Map/CodeDependency';
import { DependencyKind } from './DependencyKind';
import { layoutDependencies } from './layoutDependencies';

const kindNames: Record<DependencyKind, string> = {
    [DependencyKind.http]: 'http',
    [DependencyKind.database]: 'database',
    [DependencyKind.job]: 'job',
    [DependencyKind.queue]: 'queue'
};

export interface DependencyDiagramProps {
    readonly dependencies: readonly CodeDependency[];
}

/** What talks to what: the analyzed systems and what they call, read, write or run on a schedule. */
export const DependencyDiagram = ({ dependencies }: DependencyDiagramProps) => {
    const systems = [...new Set(dependencies.map(dependency => dependency.from))];
    const layout = layoutDependencies(systems, dependencies.map(dependency => ({ from: dependency.from, target: dependency.target, kind: dependency.kind, detail: dependency.detail })));
    const description = dependencies.map(dependency => `${dependency.from} ${dependency.detail || 'uses'} ${dependency.target}`).join('. ');

    return (
        <Panel title="What talks to what" aside="From the code">
            {dependencies.length === 0
                ? <EmptyState title="No dependencies found yet" />
                : (
                    <svg className="depmap" viewBox={`0 0 ${layout.width} ${layout.height}`} role="img" aria-label={description}>
                        <defs>
                            <marker id="dep-arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto"><path d="M0,0 L10,5 L0,10 z" className="dep-arrowhead" /></marker>
                        </defs>
                        {layout.edges.map(edge => (
                            <g key={edge.key}>
                                <line className={`dep-edge ${kindNames[edge.kind]}`} x1={edge.x1} y1={edge.y1} x2={edge.x2} y2={edge.y2} markerEnd="url(#dep-arrow)" />
                                {edge.label && <text className="dep-label" x={(edge.x1 + edge.x2) / 2 + 4} y={(edge.y1 + edge.y2) / 2 - 3}>{edge.label}</text>}
                            </g>
                        ))}
                        {layout.nodes.map(node => (
                            <g key={node.id}>
                                <rect className={`dep-node ${node.kind}`} x={node.x} y={node.y} width={node.width} height={node.height} rx={8} />
                                <text className="dep-text" x={node.x + node.width / 2} y={node.y + node.height / 2 + 4} textAnchor="middle">{node.label}</text>
                            </g>
                        ))}
                    </svg>
                )}
        </Panel>
    );
};
