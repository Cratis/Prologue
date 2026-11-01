// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { entityHistory, historyLimit } from '../history';

const at = (minute: number) => new Date(Date.UTC(2026, 10, 1, 10, minute));

const capture = (id: string, minute: number, path: string, status = 201, tables: { table: string; operation: number }[] = []) =>
    ({ capture: id, occurred: at(minute), http: { method: 'POST', path, status }, transactions: [{ tables }] });

const observation = (id: string, minute: number, path: string, outcome = 0, shadowStatus = 201) =>
    ({ observation: id, occurred: at(minute), method: 'POST', path, shape: 'POST /api/loans', legacyStatus: 201, shadowStatus, outcome });

describe('when following an entity', () => {
    const captures = [
        capture('c3', 3, '/api/loans/4412/return', 200, [{ table: 'Loans', operation: 1 }, { table: 'Inventory', operation: 1 }]),
        capture('c2', 2, '/api/members/2210'),
        capture('c1', 1, '/api/loans/4412', 201, [{ table: 'Loans', operation: 0 }])
    ];
    const parity = [observation('p1', 1, '/api/loans/4412'), observation('p9', 4, '/api/loans/44120')];
    const history = entityHistory(captures, parity, '4412');

    it('puts both systems in one timeline, oldest first', () => expect(history.map(entry => entry.key)).toEqual(['legacy-c1', 'new-p1', 'legacy-c3']));
    it('lists the legacy system before the new one at the same time', () => expect(history.slice(0, 2).map(entry => entry.source)).toEqual(['legacy', 'new']));
    it('says what the legacy system did to the tables', () => expect(history[2].how).toBe('UPDATE Loans, UPDATE Inventory'));
    it('says how the new system decided', () => expect(history[1].how).toBe('shadow · same decision'));
    it('matches whole path segments only', () => expect(history.some(entry => entry.key === 'new-p9')).toBe(false));
    it('matches without regard to case', () => expect(entityHistory([capture('c', 1, '/api/Books/ABC-123')], [], 'abc-123')).toHaveLength(1));
    it('is empty without an entity', () => expect(entityHistory(captures, parity, '  ')).toEqual([]));
    it('says when the new system did not answer', () => expect(entityHistory([], [observation('p', 1, '/api/loans/7', 2, 0)], '7')[0].what).toBe('No answer to POST /api/loans/7'));
    it('keeps the newest entries when there are more than it shows', () => {
        const many = Array.from({ length: historyLimit + 5 }, (_, index) => capture(`c${index}`, index, '/api/loans/1'));
        const result = entityHistory(many, [], '1');
        expect(result).toHaveLength(historyLimit);
        expect(result.at(-1)?.key).toBe(`legacy-c${historyLimit + 4}`);
    });
});
