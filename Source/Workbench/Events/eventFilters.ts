// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { EventCard } from './Suggestions/EventCard';
import { SuggestionOrigin } from './Suggestions/SuggestionOrigin';
import { SuggestionStatus } from './Suggestions/SuggestionStatus';

/** What the filter pills of the Events page narrow the suggestions to. */
export type EventFilter = 'all' | 'review' | 'question' | 'uncaptured' | 'accepted' | 'rejected';

export const eventFilters: readonly { readonly value: EventFilter; readonly label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'review', label: 'To review' },
    { value: 'question', label: 'Needs an answer' },
    { value: 'uncaptured', label: 'Code only / no command' },
    { value: 'accepted', label: 'Accepted' },
    { value: 'rejected', label: 'Rejected' }
];

/** Whether a suggestion rests on code alone or on writes with no command, rather than on captured commands. */
export const isUncaptured = (card: Pick<EventCard, 'origin'>): boolean => card.origin !== SuggestionOrigin.captured;

/** The chip an uncaptured suggestion carries: where its evidence comes from. Nothing for a suggestion the captures evidence. */
export const originLabel = (card: Pick<EventCard, 'origin'>): string | undefined =>
    card.origin === SuggestionOrigin.codeOnly ? 'Code only' : card.origin === SuggestionOrigin.noCommand ? 'No command' : undefined;

/** Whether a suggestion belongs under a filter. */
export const matchesFilter = (card: Pick<EventCard, 'status' | 'needsAnswer' | 'origin'>, filter: EventFilter): boolean => {
    switch (filter) {
        case 'all': return true;
        case 'review': return card.status === SuggestionStatus.suggested && !card.needsAnswer;
        case 'question': return card.needsAnswer;
        case 'uncaptured': return isUncaptured(card);
        case 'accepted': return card.status === SuggestionStatus.accepted;
        case 'rejected': return card.status === SuggestionStatus.rejected;
    }
};

/** How many suggestions each filter shows, for the count on its pill. */
export const countsByFilter = (cards: readonly Pick<EventCard, 'status' | 'needsAnswer' | 'origin'>[]): Readonly<Record<EventFilter, number>> => ({
    all: cards.length,
    review: cards.filter(card => matchesFilter(card, 'review')).length,
    question: cards.filter(card => matchesFilter(card, 'question')).length,
    uncaptured: cards.filter(card => matchesFilter(card, 'uncaptured')).length,
    accepted: cards.filter(card => matchesFilter(card, 'accepted')).length,
    rejected: cards.filter(card => matchesFilter(card, 'rejected')).length
});

/** The line beside the page title: how many of the suggestions that are not rejected are accepted. */
export const acceptedSummary = (cards: readonly Pick<EventCard, 'status'>[]): string => {
    const live = cards.filter(card => card.status !== SuggestionStatus.rejected);
    const accepted = live.filter(card => card.status === SuggestionStatus.accepted).length;
    return `${accepted} of ${live.length} accepted`;
};

/** The confidence as a whole percentage. */
export const percent = (confidence: number): number => Math.round(confidence * 100);

/** Whether the evidence is thin enough that the meter warns. */
export const isLowConfidence = (confidence: number): boolean => confidence < 0.6;
