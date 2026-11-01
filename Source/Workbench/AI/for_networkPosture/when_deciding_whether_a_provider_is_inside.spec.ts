// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { isInsideNetwork, isPrivateEndpoint } from '../networkPosture';

describe('when deciding whether an endpoint is private', () => {
    it.each([
        'http://localhost:11434/v1',
        'localhost:1234/v1',
        'http://127.0.0.1:8000/v1',
        'https://10.4.2.9/v1',
        'http://172.20.1.5:8000',
        'http://192.168.1.20:1234/v1',
        'http://gpu-box.lan/v1',
        'http://ollama:11434',
        'http://[::1]:8080/v1'
    ])('treats %s as private', endpoint => expect(isPrivateEndpoint(endpoint)).toBe(true));

    it.each([
        'https://api.openai.com/v1',
        'https://example.openai.azure.com',
        'http://172.32.0.1/v1',
        'http://8.8.8.8/v1',
        '',
        undefined
    ])('treats %s as public', endpoint => expect(isPrivateEndpoint(endpoint)).toBe(false));
});

describe('when deciding whether a provider is inside the network', () => {
    it('defaults an OpenAI-compatible provider with a private endpoint to inside', () => expect(isInsideNetwork(true, 'http://localhost:11434/v1', undefined)).toBe(true));
    it('defaults an OpenAI-compatible provider with a public endpoint to outside', () => expect(isInsideNetwork(true, 'https://api.example.com/v1', undefined)).toBe(false));
    it('never defaults another vendor to inside', () => expect(isInsideNetwork(false, 'http://localhost:11434/v1', undefined)).toBe(false));
    it('lets a mark of outside win over a private endpoint', () => expect(isInsideNetwork(true, 'http://localhost:11434/v1', false)).toBe(false));
    it('lets a mark of inside win over a public endpoint', () => expect(isInsideNetwork(false, undefined, true)).toBe(true));
});
