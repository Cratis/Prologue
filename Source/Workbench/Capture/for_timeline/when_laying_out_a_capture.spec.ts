// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { layoutTimeline } from '../timeline';
import { acceptedReturn, refusedLoan } from '../for_captureFixtures/captureFixtures';

describe('when laying out a capture', () => {
    const timeline = layoutTimeline(acceptedReturn, 2000);
    const kinds = timeline.blocks.map(block => block.kind);

    it('spans the correlation window', () => expect(timeline.spanMilliseconds).toBe(2000));
    it('puts the command in the HTTP lane', () => expect(kinds.filter(kind => kind === 'http')).toHaveLength(1));
    it('puts one block per changed table in the database lane', () => expect(kinds.filter(kind => kind === 'db')).toHaveLength(2));
    it('puts the spans in the spans lane', () => expect(kinds.filter(kind => kind === 'otel')).toHaveLength(1));
    it('places a transaction by its offset in the window', () => expect(timeline.blocks.find(block => block.kind === 'db')?.left).toBeCloseTo(6));
});

describe('when an observation lies beyond the window', () => {
    it('widens the lanes to hold it', () => {
        const late = { ...acceptedReturn, transactions: [{ ...acceptedReturn.transactions[0], offsetMilliseconds: 3000 }] };
        expect(layoutTimeline(late, 2000).spanMilliseconds).toBe(3040);
    });
});

describe('when the capture has no command', () => {
    it('leaves the HTTP lane empty', () => {
        const orphan = { ...refusedLoan, http: { ...refusedLoan.http, shape: '', method: '', path: '' }, spans: [] };
        expect(layoutTimeline(orphan, 2000).blocks.some(block => block.kind === 'http')).toBe(false);
    });
});
