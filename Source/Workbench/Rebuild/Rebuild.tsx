// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { EmptyState, LoadingState, PageHeader } from '../Components';
import { Link } from '../Shell/Router';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { AddSliceForm } from './AddSliceForm';
import { DisagreementPanel } from './DisagreementPanel';
import { DriftPanel } from './DriftPanel';
import { OneHistory } from './OneHistory';
import { RebuildSlicesTable } from './RebuildSlicesTable';
import { cutOverCount } from './stages';
import { useRebuildActions } from './useRebuildActions';
import { useRebuildData } from './useRebuildData';

/** The Rebuild step: slices move from observed to cut over, with parity against the legacy decisions and drift from the accepted model. */
export const Rebuild = () => {
    const header = useStepHeader('rebuild');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="Slices belong to a prologue and its systems." action={<Link to="/systems" className="btn primary">Go to Systems</Link>} /></>;
    return <RebuildPage />;
};

const RebuildPage = () => {
    const header = useStepHeader('rebuild');
    const prologue = useCurrentPrologue()!;
    const data = useRebuildData(prologue);
    const actions = useRebuildActions(prologue);
    const [picked, setPicked] = useState('');

    return (
        <>
            <PageHeader {...header} actions={<span className="stamp">{cutOverCount(data.slices)} of {data.slices.length} slices cut over</span>} />
            <div className="stack">
                <RebuildSlicesTable
                    slices={data.slices}
                    isBusy={actions.isBusy}
                    onAdvance={(action, name) => void actions.advance(action, name)}
                    onStepBack={name => void actions.stepBack(name)}
                    onRemove={name => void actions.remove(name)}
                    onShowDifferences={setPicked} />
                <AddSliceForm
                    knownShapes={data.knownShapes}
                    taken={data.slices.flatMap(slice => slice.shapes)}
                    busy={actions.isBusy('add-slice')}
                    onAdd={actions.add} />
                <div className="grid grid-halves">
                    <DisagreementPanel slices={data.slices} differences={data.differences} picked={picked} isBusy={actions.isBusy} onResolve={(observation, accept) => void actions.resolve(observation, accept)} />
                    <OneHistory captures={data.captures} observations={data.observations} />
                </div>
                <DriftPanel
                    accepted={data.modelAccepted}
                    items={data.driftItems}
                    isBusy={actions.isBusy}
                    onAcceptModel={() => void actions.acceptModel()}
                    onSuggest={key => void actions.suggestEvents(key)} />
            </div>
        </>
    );
};
