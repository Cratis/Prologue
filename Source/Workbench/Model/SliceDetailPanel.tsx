// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { operationLetter } from '../Capture/format';
import { Chip, Panel } from '../Components';
import { Link } from '../Shell/Router';
import { BoardSlice, sliceTypeLabel } from './board';
import { SliceDetail } from './sliceDetail';

export interface SliceDetailPanelProps {
    readonly slice: BoardSlice;
    readonly detail: SliceDetail;
}

const Flow = ({ slice }: { slice: BoardSlice }) => (
    <p className="flow">
        {slice.commands.map(command => <span key={command} className="sticky cmd inline">{command}</span>)}
        {slice.commands.length > 0 && slice.events.length > 0 && ' → '}
        {slice.events.map(event => <span key={event.key} className={`sticky evt inline${event.isOpen ? ' pending' : ''}`}>{event.name}</span>)}
    </p>
);

const Evidence = ({ detail }: { detail: SliceDetail }) => (
    <table className="facts">
        <tbody>
            <tr>
                <th scope="row">Observed as</th>
                <td>{detail.observed.length === 0 ? <span className="muted">no HTTP command</span> : detail.observed.map(item => <div key={item.shape} className="mono">{item.shape} · {item.captures}</div>)}</td>
            </tr>
            <tr>
                <th scope="row">Writes</th>
                <td>{detail.writes.length === 0 ? '—' : detail.writes.map(write => (
                    <div key={write.table}>{write.operations.map(operation => <span key={operation} className={`op ${operationLetter(operation)}`}>{operationLetter(operation)}</span>)} {write.table}</div>
                ))}</td>
            </tr>
            <tr>
                <th scope="row">Rules seen</th>
                <td>{detail.rules.length === 0 ? <span className="muted">none inferred</span> : detail.rules.map(rule => <div key={rule}>{rule}</div>)}</td>
            </tr>
            <tr>
                <th scope="row">Status</th>
                <td>
                    {detail.readyForScreenplay
                        ? <Chip variant="ok">ready for Screenplay</Chip>
                        : <><Chip variant="warn">has open suggestions</Chip> <Link to="/events">Review in Events</Link></>}
                </td>
            </tr>
        </tbody>
    </table>
);

/** What is known about the selected slice: the command and its events, the shapes it was observed as, the tables it writes, the rules seen, and whether it can go to Screenplay. */
export const SliceDetailPanel = ({ slice, detail }: SliceDetailPanelProps) => (
    <Panel title="Slice" aside={`${sliceTypeLabel(slice.type)} · ${slice.module}`}>
        {slice.readModels.length > 0 && (
            <>
                <p><b>{slice.readModels.join(', ')}</b> in {slice.module}.</p>
                <p className="muted">Prologue does not record GET traffic, so read models are inferred from the tables the commands write and the events that change them. Confirm the columns against the real screens.</p>
            </>
        )}
        {(slice.commands.length > 0 || slice.events.length > 0) && <Flow slice={slice} />}
        <Evidence detail={detail} />
    </Panel>
);
