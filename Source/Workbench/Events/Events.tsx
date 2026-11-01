// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { EmptyState, LoadingState, PageHeader, SegmentedControl } from '../Components';
import { Link } from '../Shell/Router';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { BoundariesPanel } from './BoundariesPanel';
import { EventCardView } from './EventCardView';
import { InterpretationBar, isInterpreting } from './InterpretationBar';
import { NotEventsPanel } from './NotEventsPanel';
import { QuestionsPanel } from './QuestionsPanel';
import { EventFilter, acceptedSummary, countsByFilter, eventFilters, matchesFilter } from './eventFilters';
import { useEventActions } from './useEventActions';
import { useEventsData } from './useEventsData';

/** The Events step: the interpreter's suggested events with their evidence, the questions it asks, and the consistency boundaries, for a person to decide on. */
export const Events = () => {
    const header = useStepHeader('events');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="Events are suggested from the captures of a prologue." action={<Link to="/systems" className="btn primary">Go to Systems</Link>} /></>;
    return <EventsPage />;
};

const EventsPage = () => {
    const header = useStepHeader('events');
    const prologue = useCurrentPrologue()!;
    const data = useEventsData(prologue);
    const actions = useEventActions(prologue);
    const [filter, setFilter] = useState<EventFilter>('all');

    const counts = countsByFilter(data.cards);
    const shown = data.cards.filter(card => matchesFilter(card, filter));
    const headerActions = (
        <>
            <span className="stamp">{acceptedSummary(data.cards)}</span>
            <Link to="/model" className="btn primary">Build event model</Link>
        </>
    );

    return (
        <>
            <PageHeader {...header} actions={headerActions} />
            <div className="grid grid-aside events">
                <div className="column">
                    <InterpretationBar status={data.status} busy={actions.isBusy('interpret')} onRun={() => void actions.interpret()} />
                    {!data.ready
                        ? <LoadingState label="Loading suggestions" />
                        : data.cards.length === 0
                            ? <EmptyState title="No events suggested yet" description="Run the interpretation to turn what was captured into suggested events. Capture some traffic first if Capture has not seen any." action={<Link to="/capture" className="btn">Go to Capture</Link>} />
                            : (
                                <>
                                    <SegmentedControl label="Filter suggestions" options={eventFilters.map(item => ({ value: item.value, label: `${item.label} · ${counts[item.value]}` }))} value={filter} onChange={setFilter} />
                                    <div className="evcards">
                                        {shown.map(card => (
                                            <EventCardView
                                                key={card.key}
                                                card={card}
                                                busy={actions.isBusy(`event-${card.key}`)}
                                                onAccept={key => void actions.accept(key)}
                                                onReject={key => void actions.reject(key)}
                                                onRestore={key => void actions.restore(key)}
                                                onRename={actions.rename} />
                                        ))}
                                        {shown.length === 0 && <p className="muted">Nothing in this filter.</p>}
                                    </div>
                                </>
                            )}
                </div>
                <div className="column">
                    <QuestionsPanel
                        questions={data.questions}
                        answers={data.answers}
                        busy={actions.isBusy}
                        onAnswer={(question, answer) => void actions.answer(question, answer)}
                        interpreting={isInterpreting(data.status)} />
                    <BoundariesPanel boundaries={data.boundaries} choices={data.choices} busy={actions.isBusy} onChoose={(boundary, option) => void actions.choose(boundary, option)} />
                    <NotEventsPanel notEvents={data.notEvents} />
                </div>
            </div>
        </>
    );
};
