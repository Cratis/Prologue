// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, Panel } from '../Components';
import { ScenarioStatus } from './scenarioStatus';

export interface ScenarioScriptPanelProps {
    readonly statuses: readonly ScenarioStatus[];
    readonly active: string;
    readonly adding: string;
    readonly onAdding: (value: string) => void;
    readonly onAdd: () => void;
    readonly onRemove: (name: string) => void;
    readonly isBusy: (key: string) => boolean;
}

/** The scenarios to run while capturing, each captured or not run yet, with how many captures carry its label. */
export const ScenarioScriptPanel = ({ statuses, active, adding, onAdding, onAdd, onRemove, isBusy }: ScenarioScriptPanelProps) => (
    <Panel title="Scenario script" aside="Run these in the system while capturing">
        <div className="scen">
            {statuses.length === 0 && <p className="muted small-text">No scenarios yet. Add the business scenarios you will run.</p>}
            {statuses.map(status => (
                <div className="scen-item" key={status.name}>
                    <Chip variant={status.captured ? 'ok' : 'warn'}>{status.captured ? 'Captured' : 'Not run yet'}</Chip>
                    <span className="grow">{status.name}{status.name === active && <> <Chip variant="http">running</Chip></>}</span>
                    <span className="muted num small-text">{status.captured ? `${status.captures} captures` : ''}</span>
                    <Button small variant="ghost" disabled={status.name === active || isBusy(`remove-scenario-${status.name}`)} onClick={() => onRemove(status.name)} aria-label={`Remove ${status.name}`}>Remove</Button>
                </div>
            ))}
        </div>
        <form className="marker" onSubmit={event => { event.preventDefault(); onAdd(); }}>
            <input placeholder={'Add a scenario, for example \u201cMember returns a late book\u201d'} aria-label="New scenario" value={adding} onChange={event => onAdding(event.target.value)} />
            <Button type="submit" disabled={adding.trim() === '' || isBusy('add-scenario')}>Add</Button>
        </form>
    </Panel>
);
