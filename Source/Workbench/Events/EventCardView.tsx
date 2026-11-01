// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { KeyboardEvent, useState } from 'react';
import { Button, Chip } from '../Components';
import { Link } from '../Shell/Router';
import { EventCard } from './Suggestions/EventCard';
import { NoteKind } from './Suggestions/NoteKind';
import { SuggestionStatus } from './Suggestions/SuggestionStatus';
import { isLowConfidence, percent } from './eventFilters';

export interface EventCardViewProps {
    readonly card: EventCard;
    readonly busy: boolean;
    readonly onAccept: (key: string) => void;
    readonly onReject: (key: string) => void;
    readonly onRestore: (key: string) => void;
    readonly onRename: (key: string, name: string) => Promise<boolean>;
}

const statusClass = (card: EventCard) =>
    card.status === SuggestionStatus.accepted ? 'accepted' : card.status === SuggestionStatus.rejected ? 'rejected' : card.needsAnswer ? 'question' : '';

const statusLabel = (card: EventCard) =>
    card.status === SuggestionStatus.accepted ? 'Accepted' : card.status === SuggestionStatus.rejected ? 'Rejected' : card.needsAnswer ? 'Waiting on your answer' : 'Suggested';

const statusChip = (card: EventCard) =>
    card.status === SuggestionStatus.accepted ? 'ok' : card.needsAnswer ? 'warn' : 'neutral';

/** One suggested event: its name beside the mechanical one, where it belongs, its properties, how sure the interpreter is, the evidence and the notes. */
export const EventCardView = ({ card, busy, onAccept, onReject, onRestore, onRename }: EventCardViewProps) => {
    const [editing, setEditing] = useState(false);
    const [draft, setDraft] = useState(card.name);

    const begin = () => {
        setDraft(card.name);
        setEditing(true);
    };

    const finish = async () => {
        const name = draft.trim();
        if (name && name !== card.name) await onRename(card.key, name);
        setEditing(false);
    };

    const onKey = (event: KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'Enter') void finish();
        if (event.key === 'Escape') setEditing(false);
    };

    const code = card.evidence.code;
    const tables = card.evidence.tables;
    const rejected = card.status === SuggestionStatus.rejected;
    const confidence = card.confidence;

    return (
        <article className={`ev ${statusClass(card)}${card.withdrawn ? ' withdrawn' : ''}`} data-ev={card.key}>
            <div>
                <div className="row">
                    {editing
                        ? <input autoFocus aria-label="Event name" value={draft} onChange={event => setDraft(event.target.value)} onKeyDown={onKey} onBlur={() => void finish()} />
                        : <span className="name">{card.name}</span>}
                    {card.mechanical !== card.name && <span className="mech" title="Mechanical name">{card.mechanical}</span>}
                </div>
                <div className="row tight">
                    <Chip>{card.module}</Chip>
                    <Chip>stream: {card.stream}</Chip>
                    <Chip variant={statusChip(card)}>{statusLabel(card)}</Chip>
                    {card.withdrawn && <Chip variant="drift">No longer found</Chip>}
                </div>
                <div className="props">
                    {card.properties.length > 0
                        ? card.properties.map(property => <span key={property.name} className="prop">{property.name} <i>{property.type}</i></span>)
                        : <span className="muted note">No properties. The fact and its stream identity are the whole event.</span>}
                </div>
            </div>
            <div className="actions">
                <span className="conf" title="Evidence strength">
                    <span className="meter"><i className={isLowConfidence(confidence) ? 'low' : ''} style={{ width: `${percent(confidence)}%` }} /></span>
                    {percent(confidence)}%
                </span>
                {card.status !== SuggestionStatus.accepted && !rejected && <Button small variant="primary" disabled={busy || card.withdrawn} onClick={() => onAccept(card.key)}>Accept</Button>}
                <Button small disabled={busy || editing} onClick={begin}>Rename</Button>
                {rejected
                    ? <Button small variant="ghost" disabled={busy} onClick={() => onRestore(card.key)}>Restore</Button>
                    : <Button small variant="ghost" disabled={busy} onClick={() => onReject(card.key)}>Reject</Button>}
            </div>
            {card.advice && <div className="advice">{card.advice}</div>}
            <div className="evidence">
                {tables.length > 0 && <span>Evidence: <span className="mono">{tables.map(table => `${table.operation.toUpperCase()} ${table.table} (${table.columns.join(', ')})`).join(' + ')}</span></span>}
                {code.map(place => <Link key={`${place.file}:${place.line}`} to="/discover" className="codeev">Code: {place.file}{place.line > 0 ? `:${place.line}` : ''}</Link>)}
                <span className="num">{card.evidence.captures} {card.evidence.captures === 1 ? 'capture' : 'captures'}</span>
                {card.evidence.shapes.map(shape => <Link key={shape.shape} to="/explore" className="mono">{shape.shape} ({shape.captures})</Link>)}
                {card.evidence.scenarios.map(scenario => <Chip key={scenario} variant="http">{scenario}</Chip>)}
            </div>
            {card.notes.length > 0 && (
                <div className="notes">
                    {card.notes.map(note => <div key={note.text} className="note">{note.kind === NoteKind.rule ? 'Rule: ' : note.kind === NoteKind.constraint ? 'Constraint: ' : ''}{note.text}</div>)}
                </div>
            )}
        </article>
    );
};
