// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo, useState } from 'react';
import { Field, Panel } from '../Components';
import { CaptureLike, entityHistory, ParityLike } from './history';

export interface OneHistoryProps {
    readonly captures: readonly CaptureLike[];
    readonly observations: readonly ParityLike[];
}

const when = (at: Date) => new Date(at).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit' });

/** One entity across both systems: what the legacy system was captured doing to it, and what the new system decided for the same commands. */
export const OneHistory = ({ captures, observations }: OneHistoryProps) => {
    const [entity, setEntity] = useState('');
    const entries = useMemo(() => entityHistory(captures, observations, entity), [captures, observations, entity]);

    return (
        <Panel title="One history" aside="An entity id across the legacy system and the new one">
            <div className="history-lookup">
                <Field label="Entity id" hint="The id in the path, such as 4412 in /api/loans/4412/return">
                    <input value={entity} onChange={event => setEntity(event.target.value)} placeholder="4412" />
                </Field>
            </div>
            {entity.trim() !== '' && entries.length === 0 && <p className="muted small-text">Nothing in the latest captures or parity observations mentions {entity.trim()}.</p>}
            <div className="hist">
                {entries.map(entry => (
                    <div className="h" key={entry.key}>
                        <span className="t">{when(entry.at)}</span>
                        <span className={`src-tag ${entry.source}`}>{entry.source}</span>
                        <span>{entry.what}<span className="how">{entry.how}</span></span>
                    </div>
                ))}
            </div>
        </Panel>
    );
};
