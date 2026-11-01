// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { Button, Chip, EmptyState, Panel } from '../Components';
import { decisionLabel, openDifferencesOf, sliceWithDifferences } from './differences';
import { RebuildDifference } from './Differences/RebuildDifference';
import { RebuildSlice } from './RebuildSlice';

export interface DisagreementPanelProps {
    readonly slices: readonly RebuildSlice[];
    readonly differences: readonly RebuildDifference[];
    readonly picked: string;
    readonly isBusy: (key: string) => boolean;
    readonly onResolve: (observation: Guid, accept: boolean) => void;
}

const when = (at: Date) => new Date(at).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit' });

const variantOf = (decision: number) => decision === 1 ? 'ok' : decision === 2 ? 'bad' : 'neutral';

/** Where the legacy system and the new slice decided differently: both decisions side by side, with accept or match-legacy recorded as a decision. */
export const DisagreementPanel = ({ slices, differences, picked, isBusy, onResolve }: DisagreementPanelProps) => {
    const slice = sliceWithDifferences(slices, differences, picked);
    const open = openDifferencesOf(slice, differences);
    const aside = slice ? `${slice.name} · ${open.length} open` : undefined;

    return (
        <Panel title="Where they disagree" aside={aside}>
            {!slice
                ? <EmptyState title="No open differences" description="The legacy system and the new slices have decided the same way every time they were compared." />
                : open.slice(0, 5).map(difference => (
                    <div className="difference" key={String(difference.observation)}>
                        <span className="muted where mono">{difference.shape} · {difference.path} · {when(difference.occurred)}</span>
                        <div className="diverge">
                            <div><h4>Legacy decided</h4><Chip variant={variantOf(difference.legacyDecision)}>{decisionLabel(difference.legacyDecision)}</Chip><p className="muted small-text">Answered {difference.legacyStatus}</p></div>
                            <div><h4>New slice decided</h4><Chip variant={variantOf(difference.shadowDecision)}>{decisionLabel(difference.shadowDecision)}</Chip><p className="muted small-text">Answered {difference.shadowStatus}</p></div>
                        </div>
                        <div className="row">
                            <Button small variant="primary" disabled={isBusy(`difference-${String(difference.observation)}`)} onClick={() => onResolve(difference.observation, true)}>Accept new behavior</Button>
                            <Button small disabled={isBusy(`difference-${String(difference.observation)}`)} onClick={() => onResolve(difference.observation, false)}>Match legacy</Button>
                        </div>
                    </div>
                ))}
            {open.length > 5 && <p className="muted small-text">{open.length - 5} more after these.</p>}
        </Panel>
    );
};
