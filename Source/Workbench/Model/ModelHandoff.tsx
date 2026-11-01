// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '../Components';
import { RebuildPlan } from './rebuildPlan';

export interface ModelHandoffProps {
    readonly openSuggestions: number;
    readonly plan: RebuildPlan;
    readonly busy: boolean;
    readonly onConfirm: () => void;
    readonly onCancel: () => void;
}

/** Asks before the model is accepted for rebuild while suggestions are still open: those slices stay out of Rebuild and are drafts in the .play. */
export const ModelHandoff = ({ openSuggestions, plan, busy, onConfirm, onCancel }: ModelHandoffProps) => (
    <div className="answer handoff" role="alertdialog" aria-label="Accept the model with open suggestions">
        <p>
            <b>{openSuggestions} suggestion{openSuggestions === 1 ? ' is' : 's are'} still open.</b> They stay marked as drafts in the .play file,
            and {plan.open.length === 0 ? 'no slice is held back' : `${plan.open.length} slice${plan.open.length === 1 ? '' : 's'} (${plan.open.join(', ')}) will not be added to Rebuild`}.
            {' '}{plan.create.length} slice{plan.create.length === 1 ? '' : 's'} will be added to Rebuild.
        </p>
        <div className="row">
            <Button variant="primary" disabled={busy} onClick={onConfirm}>Accept anyway</Button>
            <Button disabled={busy} onClick={onCancel}>Keep reviewing</Button>
        </div>
    </div>
);
