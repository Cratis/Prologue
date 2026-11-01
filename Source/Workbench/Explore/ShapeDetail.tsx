// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, Panel } from '../Components';
import { operationLetter } from '../Capture/format';
import { ShapeLike, WriteLike } from './exploreTypes';
import { handlerOf, percent, statusRows } from './matrix';

export interface ShapeDetailProps {
    readonly shape: ShapeLike;
    readonly excluded: boolean;
    readonly split: boolean;
    readonly isBusy: (key: string) => boolean;
    readonly onExclude: (shape: string) => void;
    readonly onInclude: (shape: string) => void;
    readonly onSplit: (shape: string) => void;
    readonly onUndoSplit: (shape: string) => void;
}

const Writes = ({ writes, captures }: { writes: readonly WriteLike[]; captures: number }) => (
    writes.length === 0
        ? <>none</>
        : <>{writes.flatMap(write => write.operations.map(({ operation, runs }) => (
            <div key={`${write.table}:${operation}`}>
                <span className={`op ${operationLetter(operation)}`}>{operationLetter(operation)}</span> {write.table} <span className="muted">({percent(runs, captures)}%)</span>{' '}
                <span className="mono muted">{write.columns.join(', ')}</span>
            </div>
        )))}</>
);

/** What was captured about one command shape: how it was answered, what it wrote, what is worth noticing, and the actions that change how it is interpreted. */
export const ShapeDetail = ({ shape, excluded, split, isBusy, onExclude, onInclude, onSplit, onUndoSplit }: ShapeDetailProps) => (
    <Panel title="Shape detail" aside={`${shape.captures} captures`}>
        <div className="mono shape-title">{shape.shape}</div>
        <table className="facts">
            <tbody>
                <tr><th scope="row">Handler span</th><td className="mono">{handlerOf(shape) ?? <span className="muted">no span seen</span>}</td></tr>
                <tr>
                    <th scope="row">Responses</th>
                    <td className="chips">{statusRows(shape).map(row => <Chip key={row.status} variant={row.refused ? 'bad' : 'ok'}>{row.status} · {row.share}%</Chip>)}</td>
                </tr>
                <tr><th scope="row">Writes</th><td><Writes writes={shape.writes} captures={shape.captures} /></td></tr>
                <tr>
                    <th scope="row">Labeled as</th>
                    <td className="chips">
                        {shape.scenarios.length === 0
                            ? <span className="muted">no scenario</span>
                            : shape.scenarios.map(entry => <Chip key={entry.scenario} variant="warn">{entry.scenario} · {entry.captures}</Chip>)}
                    </td>
                </tr>
            </tbody>
        </table>
        {split && (
            <div className="answer">
                <b>Split by response.</b> The interpreter treats each status as its own candidate:
                {statusRows(shape).map(row => (
                    <div key={row.status} className="split-row">
                        <Chip variant={row.refused ? 'bad' : 'ok'}>{row.status}</Chip> {row.captures} captures,{' '}
                        {shape.statuses.find(status => status.status === row.status)?.writes.map(write => write.table).join(', ') || 'no writes'}
                    </div>
                ))}
            </div>
        )}
        {shape.observations.map(observation => <p className="answer" key={observation.text}>{observation.text}</p>)}
        <div className="row detail-actions">
            <Button small disabled={isBusy(`scope-${shape.shape}`)} onClick={() => (excluded ? onInclude : onExclude)(shape.shape)}>
                {excluded ? 'Include in interpretation' : 'Exclude from interpretation'}
            </Button>
            <Button small disabled={isBusy(`split-${shape.shape}`)} onClick={() => (split ? onUndoSplit : onSplit)(shape.shape)}>
                {split ? 'Undo split' : 'Split by response'}
            </Button>
        </div>
    </Panel>
);
