// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '../Components';

export interface ScenarioMarkerProps {
    readonly scenarios: readonly string[];
    readonly active: string;
    readonly typed: string;
    readonly onTyped: (value: string) => void;
    readonly onStart: () => void;
    readonly onEnd: () => void;
    readonly busy: boolean;
}

/** Pick or type the scenario being run, then mark its start and end; captures arriving in between are labeled with it. */
export const ScenarioMarker = ({ scenarios, active, typed, onTyped, onStart, onEnd, busy }: ScenarioMarkerProps) => (
    <form className="marker" onSubmit={event => { event.preventDefault(); if (active) onEnd(); else onStart(); }}>
        <input
            list="scenario-options"
            placeholder="I'm now running\u2026 (pick a scenario)"
            aria-label="Current scenario"
            value={active || typed}
            disabled={active !== ''}
            onChange={event => onTyped(event.target.value)} />
        <datalist id="scenario-options">{scenarios.map(name => <option key={name} value={name} />)}</datalist>
        <Button type="submit" disabled={busy || (!active && typed.trim() === '')}>{active ? 'Mark end' : 'Mark start'}</Button>
    </form>
);
