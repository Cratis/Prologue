// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button } from '../../Components';
import { usagePercent } from '../poolUsage';
import { PoolView } from '../useAISetup';
import { ceilingExplanation } from './explanations';
import { usePoolActions } from './usePoolActions';

export interface PoolCeilingControlProps {
    readonly view: PoolView;
    readonly actions: ReturnType<typeof usePoolActions>;
}

/** The pool-wide daily token ceiling the dispatcher holds the pool to, and how much of it today's use has taken. */
export const PoolCeilingControl = ({ view, actions }: PoolCeilingControlProps) => {
    const [draft, setDraft] = useState('');
    const tokens = Number(draft.replace(/[\s,]/g, ''));
    const valid = Number.isInteger(tokens) && tokens > 0;
    const percent = usagePercent(view.usedToday, view.ceiling);
    const busy = actions.isBusy(`ceiling-${view.pool.id}`);

    const set = async () => {
        if (await actions.setCeiling(view, tokens)) setDraft('');
    };

    return (
        <div className="stack">
            <div className="row">
                <input aria-label={`Daily token ceiling of ${view.pool.name}`} inputMode="numeric" value={draft} placeholder={view.ceiling > 0 ? view.ceiling.toLocaleString('en-US') : 'No ceiling'} onChange={event => setDraft(event.target.value)} />
                <Button small disabled={!valid || busy} onClick={() => void set()}>Set ceiling</Button>
                {view.ceiling > 0 && <Button small variant="ghost" disabled={busy} onClick={() => void actions.clearCeiling(view)}>Remove ceiling</Button>}
            </div>
            {view.ceiling > 0 && (
                <>
                    <div className="capbar" role="img" aria-label={`${percent}% of the daily ceiling used`}><i style={{ width: `${percent}%` }} /></div>
                    <small>{view.usedToday.toLocaleString('en-US')} of {view.ceiling.toLocaleString('en-US')} tokens used today (UTC)</small>
                </>
            )}
            <span className="muted small-text">{ceilingExplanation}</span>
        </div>
    );
};
