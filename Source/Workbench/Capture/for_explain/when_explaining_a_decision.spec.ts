// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { ReasonSource } from '../../Decisions/ReasonSource';
import { explainDecision } from '../explain';
import { acceptedReturn, refusedLoan } from '../for_captureFixtures/captureFixtures';

describe('when explaining a refusal that wrote nothing', () => {
    const steps = explainDecision(refusedLoan, { source: ReasonSource.observed, text: 'nothing was written (409)', file: '', line: 0, capability: '' }, 2000);

    it('starts with the request and its status as evidence', () => expect(steps[0].evidence).toBe('POST /api/loans \u2192 409'));
    it('cites the missing writes', () => expect(steps[1]).toEqual({ text: 'Nothing was written.', evidence: 'No database transaction within the 2000 ms correlation window' }));
    it('points at a likely validation rule', () => expect(steps[2].text).toContain('validation rule'));
});

describe('when explaining a refusal with a rule found in the code', () => {
    const steps = explainDecision(refusedLoan, { source: ReasonSource.discovered, text: 'a loan is refused when no copy is available', file: 'LoansController.cs', line: 71, capability: 'Lending' }, 2000);

    it('states the rule', () => expect(steps[2].text).toBe('The rule behind it: a loan is refused when no copy is available.'));
    it('cites its file and line', () => expect(steps[2].evidence).toBe('LoansController.cs:71'));
});

describe('when explaining an acceptance', () => {
    const steps = explainDecision(acceptedReturn, { source: ReasonSource.observed, text: 'it wrote UPDATE Loans, UPDATE Inventory', file: '', line: 0, capability: '' }, 2000);

    it('names the tables written', () => expect(steps[1]).toEqual({ text: 'It wrote to 2 tables.', evidence: 'UPDATE Loans, UPDATE Inventory' }));
    it('ends with the observed pattern', () => expect(steps[2].text).toBe('Observed: it wrote UPDATE Loans, UPDATE Inventory.'));
});
