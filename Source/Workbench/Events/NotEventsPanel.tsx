// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Chip, Panel } from '../Components';
import { NotEvent } from './NotEvents/NotEvent';

/** What the evidence shows is not an event: derived state, nullable columns that become facts of their own, and the traffic left out. */
export const NotEventsPanel = ({ notEvents }: { readonly notEvents: readonly NotEvent[] }) => (
    <Panel title="Not events">
        {notEvents.length > 0
            ? (
                <ul className="review-list">
                    {notEvents.map(notEvent => (
                        <li key={notEvent.subject}>
                            <Chip>{notEvent.subject}</Chip>
                            <span className="muted">{notEvent.reason}</span>
                        </li>
                    ))}
                </ul>
            )
            : <p className="muted small-print">Nothing was ruled out yet. Derived counters, nullable columns that are set later and excluded traffic show up here.</p>}
    </Panel>
);
