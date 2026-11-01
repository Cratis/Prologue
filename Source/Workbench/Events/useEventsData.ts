// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo } from 'react';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { ObserveInterpretationStatus } from './Interpreting/InterpretationStatus';
import { ObserveOpenQuestions } from './Questions/OpenQuestion';
import { ObserveEventCards } from './Suggestions/EventCard';
import { ObserveEventReview } from './Suggestions/EventReview';

/**
 * What the Events page shows for a prologue: the suggested events, the questions that wait, the derived consistency boundaries with the
 * choices made, what is not an event, and how the interpretation is going. A prologue nothing was interpreted for reads as empty lists.
 */
export const useEventsData = (prologue: PrologueDefinition) => {
    const parameters = { id: prologue.id };
    const [cards] = ObserveEventCards.use(parameters);
    const [questions] = ObserveOpenQuestions.use(parameters);
    const [review] = ObserveEventReview.use(parameters);
    const [status] = ObserveInterpretationStatus.use(parameters);

    const cardData = cards.data;
    const questionData = questions.data;
    const reviewData = review.data;
    const statusData = status.data;

    return useMemo(() => ({
        ready: cards.isReady,
        cards: Array.isArray(cardData) ? cardData : [],
        questions: Array.isArray(questionData) ? questionData : [],
        boundaries: reviewData?.boundaries ?? [],
        choices: reviewData?.choices ?? [],
        answers: reviewData?.answers ?? [],
        notEvents: reviewData?.notEvents ?? [],
        status: statusData
    }), [cards.isReady, cardData, questionData, reviewData, statusData]);
};

export type EventsData = ReturnType<typeof useEventsData>;
