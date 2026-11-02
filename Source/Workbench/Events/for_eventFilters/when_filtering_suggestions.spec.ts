// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { acceptedSummary, countsByFilter, isLowConfidence, isUncaptured, matchesFilter, originLabel, percent } from '../eventFilters';
import { SuggestionOrigin } from '../Suggestions/SuggestionOrigin';
import { SuggestionStatus } from '../Suggestions/SuggestionStatus';

const suggested = { status: SuggestionStatus.suggested, needsAnswer: false, origin: SuggestionOrigin.captured };
const asked = { status: SuggestionStatus.suggested, needsAnswer: true, origin: SuggestionOrigin.captured };
const accepted = { status: SuggestionStatus.accepted, needsAnswer: false, origin: SuggestionOrigin.captured };
const rejected = { status: SuggestionStatus.rejected, needsAnswer: false, origin: SuggestionOrigin.captured };
const codeOnly = { status: SuggestionStatus.suggested, needsAnswer: false, origin: SuggestionOrigin.codeOnly };
const noCommand = { status: SuggestionStatus.accepted, needsAnswer: false, origin: SuggestionOrigin.noCommand };
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
        expect(countsByFilter(cards)).toEqual({ all: 5, review: 2, question: 1, uncaptured: 0, accepted: 1, rejected: 1 });
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

describe('when suggestions rest on code alone or on writes with no command', () => {
    const mixed = [...cards, codeOnly, noCommand];

    it('should show them under their own pill', () => {
        expect(mixed.filter(card => matchesFilter(card, 'uncaptured'))).toEqual([codeOnly, noCommand]);
    });

    it('should count them under their pill and under the pills of their status', () => {
        expect(countsByFilter(mixed)).toEqual({ all: 7, review: 3, question: 1, uncaptured: 2, accepted: 2, rejected: 1 });
    });

    it('should not call a captured suggestion uncaptured', () => {
        expect(isUncaptured(suggested)).toBe(false);
    });

    it('should label code-only and no-command suggestions and nothing else', () => {
        expect([originLabel(codeOnly), originLabel(noCommand), originLabel(suggested)]).toEqual(['Code only', 'No command', undefined]);
    });
});
