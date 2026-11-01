// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { acceptedSummary, countsByFilter, isLowConfidence, matchesFilter, percent } from '../eventFilters';
import { SuggestionStatus } from '../Suggestions/SuggestionStatus';

const suggested = { status: SuggestionStatus.suggested, needsAnswer: false };
const asked = { status: SuggestionStatus.suggested, needsAnswer: true };
const accepted = { status: SuggestionStatus.accepted, needsAnswer: false };
const rejected = { status: SuggestionStatus.rejected, needsAnswer: false };
const cards = [suggested, suggested, asked, accepted, rejected];

describe('when filtering suggestions', () => {
    it('should show everything under all', () => {
        expect(cards.every(card => matchesFilter(card, 'all'))).toBe(true);
    });

    it('should show what nobody decided and nobody asks about under to review', () => {
        expect(cards.filter(card => matchesFilter(card, 'review'))).toEqual([suggested, suggested]);
    });

    it('should show what waits for an answer under needs an answer', () => {
        expect(cards.filter(card => matchesFilter(card, 'question'))).toEqual([asked]);
    });

    it('should show accepted and rejected under their own pills', () => {
        expect([cards.filter(card => matchesFilter(card, 'accepted')), cards.filter(card => matchesFilter(card, 'rejected'))]).toEqual([[accepted], [rejected]]);
    });

    it('should count each pill', () => {
        expect(countsByFilter(cards)).toEqual({ all: 5, review: 2, question: 1, accepted: 1, rejected: 1 });
    });

    it('should summarize the accepted ones leaving the rejected ones out', () => {
        expect(acceptedSummary(cards)).toBe('1 of 4 accepted');
    });

    it('should round the confidence to a whole percentage', () => {
        expect(percent(0.936)).toBe(94);
    });

    it('should warn below sixty percent', () => {
        expect([isLowConfidence(0.59), isLowConfidence(0.6)]).toEqual([true, false]);
    });
});
