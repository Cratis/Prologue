// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { ChangeOperation } from '../../ChangeOperation';
import { operationLetter, operationName, serviceLabel, traceLinkedShare } from '../format';

describe('when working out the trace-linked share', () => {
    it('is undefined without captures that have a command', () => expect(traceLinkedShare(0, 0)).toBeUndefined());
    it('rounds the share of captures with a command', () => expect(traceLinkedShare(2, 3)).toBe(67));
});

describe('when naming a change', () => {
    it('names each operation', () => expect([ChangeOperation.insert, ChangeOperation.update, ChangeOperation.delete].map(operationName)).toEqual(['INSERT', 'UPDATE', 'DELETE']));
    it('uses the first letter for the badge', () => expect(operationLetter(ChangeOperation.delete)).toBe('D'));
});

describe('when labeling a service', () => {
    it('names the default service', () => expect(serviceLabel('')).toBe('Default service'));
    it('keeps a named service', () => expect(serviceLabel('Lending')).toBe('Lending'));
});
