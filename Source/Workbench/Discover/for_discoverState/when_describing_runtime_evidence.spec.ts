// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { DecisionKind } from '../DecisionKind';
import { DecisionRuntime } from '../Runtime/DecisionRuntime';
import { describeHappened, runtimeChip } from '../discoverState';

describe('when describing how often a decision point happened', () => {
    it('counts refusals with the status they happened with', () =>
        expect(describeHappened({ kind: DecisionKind.refusal, happened: 72, status: 409, runtime: DecisionRuntime.matches })).toBe('72 refusals (409)'));
    it('uses the singular for one refusal', () =>
        expect(describeHappened({ kind: DecisionKind.refusal, happened: 1, status: 409, runtime: DecisionRuntime.matches })).toBe('1 refusal (409)'));
    it('counts accepted commands for a state change', () =>
        expect(describeHappened({ kind: DecisionKind.stateChange, happened: 902, status: 0, runtime: DecisionRuntime.matches })).toBe('902 accepted'));
    it('counts the writes a schedule explains', () =>
        expect(describeHappened({ kind: DecisionKind.schedule, happened: 14, status: 0, runtime: DecisionRuntime.explainsWrites })).toBe('14 uncorrelated writes'));
    it('says never seen for a decision point that was never exercised', () =>
        expect(describeHappened({ kind: DecisionKind.refusal, happened: 0, status: 0, runtime: DecisionRuntime.codeOnly })).toBe('never seen'));
});

describe('when choosing the chip of a decision point', () => {
    it('matches what happened at runtime', () => expect(runtimeChip(DecisionRuntime.matches)).toEqual({ variant: 'ok', label: 'Matches' }));
    it('flags a decision point never exercised as code only', () => expect(runtimeChip(DecisionRuntime.codeOnly)).toEqual({ variant: 'warn', label: 'Code only' }));
    it('says a schedule explains writes', () => expect(runtimeChip(DecisionRuntime.explainsWrites).label).toBe('Explains uncorrelated writes'));
});
