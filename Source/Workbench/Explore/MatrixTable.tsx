// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, Panel } from '../Components';
import { operationLetter } from '../Capture/format';
import { ShapeLike, UncorrelatedLike } from './exploreTypes';
import { cellOperations, orderedShapes, tableColumns, tableKey } from './matrix';
import { uncorrelatedKey } from './uncorrelatedKey';

export interface MatrixTableProps {
    readonly shapes: readonly ShapeLike[];
    readonly uncorrelated: readonly UncorrelatedLike[];
    readonly excluded: ReadonlySet<string>;
    readonly split: ReadonlySet<string>;
    readonly selected: string;
    readonly onSelect: (key: string) => void;
    readonly onExclude: (shape: string) => void;
    readonly onInclude: (shape: string) => void;
    readonly isBusy: (key: string) => boolean;
}

/** One row per command shape and one column per table, with the kinds of change in each cell and the faded ones marking writes some runs skip. */
export const MatrixTable = ({ shapes, uncorrelated, excluded, split, selected, onSelect, onExclude, onInclude, isBusy }: MatrixTableProps) => {
    const columns = tableColumns(shapes, uncorrelated.map(table => table.write));
    const outside = uncorrelated.reduce((sum, table) => sum + table.transactions, 0);
    const outsideTables = new Map(uncorrelated.map(table => [tableKey(table.write), table]));

    return (
        <Panel
            title="Command × table"
            aside={<><span className="op I">I</span> insert <span className="op U">U</span> update <span className="op D">D</span> delete · faded = some runs</>}>
            <div className="scroll-x">
                <table className="heat">
                    <thead>
                        <tr>
                            <th className="rowh" scope="col">Command shape</th>
                            <th scope="col">Captures</th>
                            {columns.map(column => <th className="rot" scope="col" key={column.key}><span>{column.label}</span></th>)}
                            <th scope="col"><span className="visually-hidden">Exclude</span></th>
                        </tr>
                    </thead>
                    <tbody>
                        {orderedShapes(shapes).map(shape => {
                            const isExcluded = excluded.has(shape.shape);
                            return (
                                <tr key={shape.shape} className={['click', selected === shape.shape ? 'sel' : '', isExcluded ? 'excluded' : ''].filter(Boolean).join(' ')} onClick={() => onSelect(shape.shape)}>
                                    <td className="rowh">
                                        <span className="mono">{shape.shape}</span>
                                        {split.has(shape.shape) && <> <Chip variant="drift">split</Chip></>}
                                    </td>
                                    <td className="num">{shape.captures}</td>
                                    {columns.map(column => (
                                        <td key={column.key}>
                                            <span className="cell">
                                                {cellOperations(shape, column).map(cell => (
                                                    <span key={cell.operation} className={`op ${cell.letter}${cell.partial ? ' p' : ''}`} title={cell.title}>{cell.letter}</span>
                                                ))}
                                            </span>
                                        </td>
                                    ))}
                                    <td>
                                        <Button
                                            small
                                            variant="ghost"
                                            disabled={isBusy(`scope-${shape.shape}`)}
                                            onClick={event => { event.stopPropagation(); (isExcluded ? onInclude : onExclude)(shape.shape); }}>
                                            {isExcluded ? 'Include' : 'Exclude'}
                                        </Button>
                                    </td>
                                </tr>
                            );
                        })}
                        {outside > 0 && (
                            <tr className={`click${selected === uncorrelatedKey ? ' sel' : ''}`} onClick={() => onSelect(uncorrelatedKey)}>
                                <td className="rowh"><Chip variant="warn">no command</Chip> <span className="muted">standalone transactions</span></td>
                                <td className="num">{outside}</td>
                                {columns.map(column => {
                                    const table = outsideTables.get(column.key);
                                    return (
                                        <td key={column.key}>
                                            <span className="cell">
                                                {table?.write.operations.map(({ operation }) => (
                                                    <span key={operation} className={`op ${operationLetter(operation)}`}>{operationLetter(operation)}</span>
                                                ))}
                                            </span>
                                        </td>
                                    );
                                })}
                                <td />
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </Panel>
    );
};
