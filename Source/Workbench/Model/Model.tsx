// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, CopyButton, EmptyState, LoadingState, PageHeader, Panel } from '../Components';
import { Link } from '../Shell/Router';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { boardSummary } from './board';
import { ModelBoard } from './ModelBoard';
import { ModelHandoff } from './ModelHandoff';
import { SliceDetailPanel } from './SliceDetailPanel';
import { planRebuildSlices } from './rebuildPlan';
import { sliceDetail } from './sliceDetail';
import { useModelActions } from './useModelActions';
import { useModelData } from './useModelData';

/** The Model step: the provisional event model laid out as a board, and its Screenplay. */
export const Model = () => {
    const header = useStepHeader('model');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="The model is built from the events accepted for a prologue." action={<Link to="/systems" className="btn primary">Go to Systems</Link>} /></>;
    return <ModelPage />;
};

const ModelPage = () => {
    const header = useStepHeader('model');
    const prologue = useCurrentPrologue()!;
    const data = useModelData(prologue);
    const actions = useModelActions(prologue, data.modelAccepted);
    const [picked, setPicked] = useState('');
    const [confirming, setConfirming] = useState(false);

    const { board } = data;
    const selected = board.slices.find(slice => slice.key === picked) ?? board.slices[0];
    const plan = planRebuildSlices(board.slices.map(slice => ({ name: slice.name, shapes: slice.slice.shapes, isOpen: slice.isOpen })), data.tracked);
    const openSuggestions = data.model?.open ?? 0;
    const busy = actions.isBusy('accept-model');

    const accept = async () => {
        setConfirming(false);
        await actions.acceptForRebuild(plan);
    };

    const headerActions = (
        <>
            <CopyButton text={data.play} label="Copy .play" />
            <a className="btn" href={`/prologues/${String(prologue.id)}/model.play`} download="model.play">Download .play</a>
            <Button variant="primary" disabled={busy || board.slices.length === 0} onClick={() => openSuggestions > 0 ? setConfirming(true) : void accept()}>
                {data.modelAccepted ? 'Add slices to Rebuild' : 'Accept model for rebuild'}
            </Button>
        </>
    );

    return (
        <>
            <PageHeader {...header} actions={headerActions} />
            {!data.ready
                ? <LoadingState label="Loading the model" />
                : board.slices.length === 0
                    ? <EmptyState title="Nothing in the model yet" description="Accept the events the interpreter suggested and they appear here as slices." action={<Link to="/events" className="btn primary">Go to Events</Link>} />
                    : (
                        <div className="stack">
                            {confirming && <ModelHandoff openSuggestions={openSuggestions} plan={plan} busy={busy} onConfirm={() => void accept()} onCancel={() => setConfirming(false)} />}
                            <Panel>
                                <div className="row board-head">
                                    <div className="legend">
                                        <span><i className="swatch screen" />Screen (inferred)</span>
                                        <span><i className="swatch cmd" />Command</span>
                                        <span><i className="swatch evt" />Event</span>
                                        <span><i className="swatch rm" />Read model</span>
                                        <span><i className="swatch open" />Open suggestion</span>
                                    </div>
                                    <span className="stamp">{boardSummary(board)}</span>
                                </div>
                                <ModelBoard board={board} selected={selected.key} onSelect={setPicked} />
                            </Panel>
                            <div className="grid grid-halves">
                                <SliceDetailPanel slice={selected} detail={sliceDetail(selected, data.counts, data.matrix)} />
                                <Panel title={`${data.model?.systemName || 'Model'}.play`} aside={data.drafts > 0 ? `${data.drafts} draft slice${data.drafts === 1 ? '' : 's'} marked` : 'Generated from accepted suggestions'}>
                                    <pre className="code" aria-label="Generated Screenplay">{data.play}</pre>
                                </Panel>
                            </div>
                        </div>
                    )}
        </>
    );
};
