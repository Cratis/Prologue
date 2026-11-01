// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { canStepBack, cutOverCount, nextAction, pipeline } from '../stages';

const slice = (stage: number, matched = 0, mismatched = 0, open = 0) => ({ stage, matched, mismatched, openDifferences: Array.from({ length: open }, () => 'difference') });

describe('when choosing the next action', () => {
    it('marks an observed slice modeled', () => expect(nextAction(slice(0))?.label).toBe('Mark modeled'));
    it('marks a modeled slice built', () => expect(nextAction(slice(1))?.label).toBe('Mark built'));
    it('starts shadowing a built slice', () => expect(nextAction(slice(2))?.label).toBe('Start shadowing'));
    it('offers nothing for a slice that is cut over', () => expect(nextAction(slice(4))).toBeUndefined());
    it('does not block the steps before shadowing', () => expect(nextAction(slice(2))?.blocked).toBeUndefined());
    it('blocks the cut over until something has been compared', () => expect(nextAction(slice(3))?.blocked).toBe('Nothing has been compared yet'));
    it('blocks the cut over while differences are unresolved', () => expect(nextAction(slice(3, 899, 3, 3))?.blocked).toBe('3 differences are unresolved'));
    it('says it in the singular for one difference', () => expect(nextAction(slice(3, 10, 1, 1))?.blocked).toBe('1 difference is unresolved'));
    it('offers the cut over once every difference is resolved', () => expect(nextAction(slice(3, 899, 3, 0))).toMatchObject({ action: 'cutover', primary: true, blocked: undefined }));
});

describe('when drawing the pipeline', () => {
    it('marks the stages passed and the current one', () => expect(pipeline(2).map(step => step.state)).toEqual(['on', 'on', 'cur', '', '']));
    it('has passed every stage once cut over', () => expect(pipeline(4).map(step => step.state)).toEqual(['on', 'on', 'on', 'on', 'on']));
    it('can step back from every stage but the first', () => expect([0, 1, 4].map(stage => canStepBack({ stage }))).toEqual([false, true, true]));
    it('counts the slices cut over', () => expect(cutOverCount([{ stage: 4 }, { stage: 3 }, { stage: 4 }])).toBe(2));
});
