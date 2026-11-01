// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Link } from '../Shell/Router';
import { Chip, EmptyState, Panel } from '../Components';
import { AnalysisState } from './AnalysisState';
import { AnalysisStatus } from './Map/AnalysisStatus';
import { ApplicationMap } from './Map/ApplicationMap';
import { MapCapability } from './Map/MapCapability';
import { RuntimeStatus } from './Runtime/RuntimeStatus';
import { isActive } from './discoverState';
import { useCapabilitySelection } from './useCapabilitySelection';

export interface ApplicationMapPanelProps {
    readonly map?: ApplicationMap;
    readonly statuses: readonly AnalysisStatus[];

    /** A read-only view has no analysis to run and no scenario to start. */
    readonly readOnly?: boolean;
}

const chipClass = (capability: MapCapability, pending: boolean) => `cap-chip ${pending ? 'pending' : capability.runtime === RuntimeStatus.seen ? 'seen' : 'codeonly'}`;

/** The application map: each system with its modules and capabilities, seen at runtime (solid) or found only in code (dashed). */
export const ApplicationMapPanel = ({ map, statuses, readOnly = false }: ApplicationMapPanelProps) => {
    const { selected, select } = useCapabilitySelection(map);
    const pendingSubjects = new Set(statuses.filter(status => isActive(status.state) || status.state === AnalysisState.queued).map(status => String(status.subject)));

    return (
        <Panel title="Application map" aside="Solid: seen at runtime · dashed: found only in code">
            {!map || map.systems.length === 0
                ? <EmptyState title="Nothing mapped yet" description="Analyze a system or a folder and its capabilities appear here." />
                : (
                    <div className="map">
                        {map.systems.map(system => (
                            <div key={String(system.subject)} className="mapsys">
                                <h4>{system.name}</h4>
                                {system.modules.map(module => (
                                    <div key={module.name} className="mapmod">
                                        <b>{module.name}</b>
                                        <div className="caps">
                                            {module.capabilities.map(capability => {
                                                const key = `${String(system.subject)}|${module.name}|${capability.capability}`;
                                                return (
                                                    <button key={key} type="button" className={chipClass(capability, pendingSubjects.has(String(system.subject)))} aria-pressed={selected?.key === key} onClick={() => select({ key, capability })}>
                                                        {capability.capability}
                                                    </button>
                                                );
                                            })}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ))}
                    </div>
                )}
            {selected && (
                <div className="cap-detail">
                    <div className="mono">{selected.capability.file}</div>
                    <div>
                        {selected.capability.shapes.length > 0 ? selected.capability.shapes.map(shape => <code key={shape} className="mono block">{shape}</code>) : <span className="muted">No HTTP command reaches this; it runs from a job or a handler.</span>}
                    </div>
                    <div className="row">
                        {selected.capability.runtime === RuntimeStatus.seen
                            ? <Chip variant="ok">{`Seen ${selected.capability.seen.toLocaleString('en-US')} times at runtime`}</Chip>
                            : <Chip variant="warn">Found only in code</Chip>}
                        {!readOnly && selected.capability.runtime === RuntimeStatus.codeOnly && <Link to="/capture" className="btn small ghost">Run a scenario</Link>}
                    </div>
                </div>
            )}
        </Panel>
    );
};
