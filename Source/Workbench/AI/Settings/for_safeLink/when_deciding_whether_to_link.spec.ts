// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { isHttpsUrl } from '../safeLink';

describe('when deciding whether an address may be a link', () => {
    it('links an https address', () => expect(isHttpsUrl('https://github.com/login/device')).toBe(true));
    it('does not link an http address', () => expect(isHttpsUrl('http://github.com/login/device')).toBe(false));
    it('does not link a javascript address', () => expect(isHttpsUrl('javascript:alert(1)')).toBe(false));
    it('does not link a data address', () => expect(isHttpsUrl('data:text/html,hi')).toBe(false));
    it('does not link something that is not an address', () => expect(isHttpsUrl('not a url')).toBe(false));
    it('does not link nothing', () => expect(isHttpsUrl('')).toBe(false));
});
