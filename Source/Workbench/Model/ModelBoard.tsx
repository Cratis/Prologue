// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';
import { ExtractedSliceType } from '../ExtractedSliceType';
import { Board, BoardSlice, eventsOfStream, sliceTypeLabel } from './board';
import { screenEvidence } from './screens';

export interface ModelBoardProps {
    readonly board: Board;
    readonly selected: string;
    readonly onSelect: (key: string) => void;
}

interface RowProps {
    readonly label: string;
    readonly board: Board;
    readonly selected: string;
    readonly onSelect: (key: string) => void;
    readonly cell: (slice: BoardSlice) => ReactNode;
}

const Row = ({ label, board, selected, onSelect, cell }: RowProps) => (
    <>
        <div className="lh" role="rowheader">{label}</div>
        {board.slices.map(slice => (
            <div key={slice.key} className={`cell2${slice.key === selected ? ' selcol' : ''}`} onClick={() => onSelect(slice.key)}>{cell(slice)}</div>
        ))}
    </>
);

const commandCell = (slice: BoardSlice) => slice.commands.map(command => (
    <div key={command} className={`sticky ${slice.type === ExtractedSliceType.automation ? 'ui' : 'cmd'}`}>
        {slice.type === ExtractedSliceType.automation ? `⚙ ${command}` : command}
    </div>
));

/**
 * The provisional event model as a board: modules as column groups over one column per slice, and rows for screens (inferred from routes),
 * commands, events (one row per stream) and read models. A slice or event still open is outlined as pending.
 */
export const ModelBoard = ({ board, selected, onSelect }: ModelBoardProps) => (
    <div className="board-wrap">
        <div className="board" role="table" aria-label="Event model" style={{ gridTemplateColumns: `120px repeat(${board.slices.length}, 156px)` }}>
            <div className="lh head" role="columnheader">Module</div>
            {board.modules.map(module => <div key={module.name} className="mod" style={{ gridColumn: `span ${module.slices.length}` }}>{module.name}</div>)}
            <div className="lh" role="rowheader">Slice</div>
            {board.slices.map(slice => (
                <button key={slice.key} type="button" className={`slh${slice.key === selected ? ' sel' : ''}${slice.isOpen ? ' pending' : ''}`} aria-pressed={slice.key === selected} onClick={() => onSelect(slice.key)}>
                    {sliceTypeLabel(slice.type)} · {slice.name}
                </button>
            ))}
            <Row label="Screens" board={board} selected={selected} onSelect={onSelect} cell={slice => slice.screen && <div className="sticky ui">{slice.screen}<small>{screenEvidence}</small></div>} />
            <Row label="Commands" board={board} selected={selected} onSelect={onSelect} cell={commandCell} />
            {board.streams.map(stream => (
                <Row key={stream} label={`Events · ${stream}`} board={board} selected={selected} onSelect={onSelect} cell={slice => eventsOfStream(slice, stream).map(event => (
                    <div key={event.key} className={`sticky evt${event.isOpen ? ' pending' : ''}`}>
                        {event.name}
                        {event.properties.length > 0 && <small>{event.properties.map(property => property.name).join(', ')}</small>}
                    </div>
                ))} />
            ))}
            <Row label="Read models" board={board} selected={selected} onSelect={onSelect} cell={slice => slice.readModels.map(readModel => (
                <div key={readModel} className="sticky rm">{readModel}<small>no GET evidence</small></div>
            ))} />
        </div>
    </div>
);
