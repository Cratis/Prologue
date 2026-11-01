// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Chip, EmptyState, Panel } from '../Components';
import { BusinessDecision } from '../Decisions/BusinessDecision';
import { DecisionOutcome } from '../Decisions/DecisionOutcome';
import { ReasonSource } from '../Decisions/ReasonSource';

const shown = 8;

const because = (decision: BusinessDecision) => {
    const { reason } = decision;
    const place = reason.source === ReasonSource.discovered ? ` (${reason.file}:${reason.line})` : '';
    return `${reason.text}${place}`;
};

/** The decisions the system made, newest first, each with the reason it made it and the capability it belongs to. */
export const BusinessEvents = ({ decisions }: { decisions: readonly BusinessDecision[] }) => (
    <Panel title="Business events, with reasons" aside="Intent and result joined, matched to the map">
        {decisions.length === 0
            ? <EmptyState title="No decisions yet" description="A decision appears when a command has been answered." />
            : (
                <div className="biz">
                    {decisions.slice(0, shown).map(decision => {
                        const refused = decision.outcome === DecisionOutcome.refused;
                        return (
                            <div className="b" key={String(decision.capture)}>
                                <b>{decision.shape}</b> <span className="muted mono small-text">{decision.status}</span>
                                <div><span className="because">because</span> {because(decision)}</div>
                                <div className="meta">
                                    <Chip variant={refused ? 'bad' : 'ok'}>{refused ? 'Refused' : 'Accepted'}</Chip>
                                    {decision.reason.capability && <Chip>{decision.reason.capability}</Chip>}
                                    {decision.scenario && <Chip variant="warn">{decision.scenario}</Chip>}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
    </Panel>
);
