// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, EmptyState, Panel } from '../Components';
import { RebuildSlice } from './RebuildSlice';
import { parityNote, parityOf } from './parity';
import { canStepBack, nextAction, pipeline, SliceAction } from './stages';

export interface RebuildSlicesTableProps {
    readonly slices: readonly RebuildSlice[];
    readonly isBusy: (key: string) => boolean;
    readonly onAdvance: (action: SliceAction, name: string) => void;
    readonly onStepBack: (name: string) => void;
    readonly onRemove: (name: string) => void;
    readonly onShowDifferences: (name: string) => void;
}

/** The slices being rebuilt, each with its pipeline from observed to cut over, its parity with the legacy decisions and its next step. */
export const RebuildSlicesTable = ({ slices, isBusy, onAdvance, onStepBack, onRemove, onShowDifferences }: RebuildSlicesTableProps) => (
    <Panel title="Slices" aside="Parity is measured against what the legacy system decided">
        {slices.length === 0
            ? <EmptyState title="No slices yet" description="Add the first slice below: a name and the legacy command it replaces." />
            : (
                <div className="scroll-x">
                    <table className="data">
                        <caption className="visually-hidden">Slices being rebuilt</caption>
                        <thead>
                            <tr><th scope="col">Slice</th><th scope="col">Legacy route</th><th scope="col">Progress</th><th scope="col">Parity</th><th scope="col"><span className="visually-hidden">Actions</span></th></tr>
                        </thead>
                        <tbody>
                            {slices.map(slice => <SliceRow key={slice.name} slice={slice} {...{ isBusy, onAdvance, onStepBack, onRemove, onShowDifferences }} />)}
                        </tbody>
                    </table>
                </div>
            )}
    </Panel>
);

const SliceRow = ({ slice, isBusy, onAdvance, onStepBack, onRemove, onShowDifferences }: { slice: RebuildSlice } & Omit<RebuildSlicesTableProps, 'slices'>) => {
    const parity = parityOf(slice);
    const next = nextAction(slice);
    const busy = isBusy(`slice-${slice.name}`);
    const note = parityNote(parity);

    return (
        <tr>
            <td><b>{slice.name}</b></td>
            <td className="mono">{slice.shapes.map(shape => <div key={shape}>{shape}</div>)}</td>
            <td>
                <div className="pipeline" aria-label={`Progress of ${slice.name}`}>
                    {pipeline(slice.stage).map(step => <span key={step.label} className={step.state || undefined}>{step.label}</span>)}
                </div>
            </td>
            <td>
                {parity.compared === 0
                    ? <span className="muted small-text">{note || 'Not compared yet'}</span>
                    : (
                        <>
                            <span className="parity">
                                <span className={parity.percent < 100 ? 'bar partial' : 'bar'}><i style={{ width: `${parity.percent}%` }} /></span>
                                <span className="num">{parity.matched}/{parity.compared}</span>
                                {parity.open > 0 && <Button small onClick={() => onShowDifferences(slice.name)}>{parity.open} differ</Button>}
                            </span>
                            {note && <span className="muted small-text">{note}</span>}
                        </>
                    )}
            </td>
            <td>
                <div className="slice-actions">
                    {next?.blocked && <Chip variant="warn">{next.blocked}</Chip>}
                    {next && <Button small variant={next.primary ? 'primary' : 'default'} disabled={busy || next.blocked !== undefined} onClick={() => onAdvance(next.action, slice.name)}>{next.label}</Button>}
                    {canStepBack(slice) && <Button small variant="ghost" disabled={busy} onClick={() => onStepBack(slice.name)}>Step back</Button>}
                    {slice.stage < 4 && <Button small variant="ghost" disabled={busy} onClick={() => onRemove(slice.name)}>Remove</Button>}
                </div>
            </td>
        </tr>
    );
};
