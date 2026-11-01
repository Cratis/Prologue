// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { EmptyState, Field, Panel } from '../../Components';
import { PrologueDefinition } from '../../Prologues/PrologueDefinition';
import { CorrelationAndOutputCard } from '../../Prologues/CorrelationAndOutputCard';
import { DatabaseSourceCard } from './DatabaseSourceCard';
import { HttpSourceCard } from './HttpSourceCard';
import { TelemetryCard } from './TelemetryCard';

/** What is captured from each system while it runs, one system at a time, and how captures are correlated and delivered. */
export const RuntimeSourcesPanel = ({ prologue }: { prologue: PrologueDefinition }) => {
    const [chosen, setChosen] = useState('');
    const system = prologue.systems.find(candidate => String(candidate.id) === chosen) ?? prologue.systems[0];

    return (
        <Panel title="Runtime sources" aside="For Capture. Selected metadata only. Row values and request bodies are never read.">
            {!system
                ? <EmptyState title="Add a system first" description="Runtime sources belong to a system." />
                : (
                    <div className="grid" key={String(system.id)}>
                        {prologue.systems.length > 1 && (
                            <Field label="System">
                                <select value={String(system.id)} onChange={event => setChosen(event.target.value)}>
                                    {prologue.systems.map(candidate => <option key={String(candidate.id)} value={String(candidate.id)}>{candidate.name}</option>)}
                                </select>
                            </Field>
                        )}
                        <div className="grid grid-halves">
                            <HttpSourceCard prologue={prologue.id} system={system} />
                            <DatabaseSourceCard prologue={prologue.id} system={system} />
                            <TelemetryCard prologue={prologue.id} system={system} />
                            <CorrelationAndOutputCard prologue={prologue} />
                        </div>
                    </div>
                )}
        </Panel>
    );
};
