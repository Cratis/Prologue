// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { commitNavigation } from '../navigation';

const history = (pathname: string) => {
    const calls: string[] = [];
    return {
        calls,
        pathname: () => pathname,
        pushState: (_: unknown, __: string, url: string) => calls.push(`push ${url}`),
        replaceState: (_: unknown, __: string, url: string) => calls.push(`replace ${url}`)
    };
};

describe('when navigating', () => {
    it('adds a history entry', () => {
        const h = history('/systems');
        expect(commitNavigation(h, '/capture')).toBe(true);
        expect(h.calls).toEqual(['push /capture']);
    });

    it('replaces the entry for a redirect so Back does not return to it', () => {
        const h = history('/nowhere');
        expect(commitNavigation(h, '/systems', { replace: true })).toBe(true);
        expect(h.calls).toEqual(['replace /systems']);
    });

    it('does nothing when already there', () => {
        const h = history('/systems');
        expect(commitNavigation(h, '/systems')).toBe(false);
        expect(h.calls).toEqual([]);
    });
});
