// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { executeCommand, failureMessage, rejectionMessage } from '../commandFailures';

const result = (validation: string[], exceptions: string[], authorization = '') => ({
    validationResults: validation.map(message => ({ message })),
    exceptionMessages: exceptions,
    authorizationFailureReason: authorization
});

describe('when a command did not succeed', () => {
    it('says the first refusal', () => expect(failureMessage(result(['Name is required'], ['boom']))).toBe('Name is required'));
    it('says the exception when nothing was refused', () => expect(failureMessage(result([], ['boom']))).toBe('boom'));
    it('says the authorization failure', () => expect(failureMessage(result([], [], 'Not allowed'))).toBe('Not allowed'));
    it('has something to say even when the result says nothing', () => expect(failureMessage(result([], []))).toBe('The command did not succeed'));
});

describe('when a command could not be sent', () => {
    it('shows a short reason', () => expect(rejectionMessage(new TypeError('Failed to fetch'))).toBe('The command could not be sent: Failed to fetch'));
    it('leaves out a reason that is long', () => expect(rejectionMessage(new Error('x'.repeat(500)))).toBe('The command could not be sent'));
    it('leaves out a reason that spans lines like a stack', () => expect(rejectionMessage(new Error('failed\n    at somewhere (file.ts:1)'))).toBe('The command could not be sent'));
    it('copes with a rejection that is not an error', () => expect(rejectionMessage('nope')).toBe('The command could not be sent'));
});

describe('when executing a command', () => {
    const answer = (isSuccess: boolean) => ({ isSuccess, ...result(isSuccess ? [] : ['No'], []) });

    it('reports success with the message to show', async () =>
        expect(await executeCommand({ execute: async () => answer(true) }, 'Saved')).toEqual({ succeeded: true, message: 'Saved' }));
    it('reports a refusal', async () =>
        expect(await executeCommand({ execute: async () => answer(false) })).toEqual({ succeeded: false, message: 'No' }));
    it('reports a rejection instead of throwing', async () =>
        expect(await executeCommand({ execute: () => Promise.reject(new TypeError('Failed to fetch')) })).toEqual({ succeeded: false, message: 'The command could not be sent: Failed to fetch' }));
});
