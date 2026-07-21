// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';
import { RequestFailed } from './RequestFailed';

// Requests are same-origin and relative. Vite forwards /api to the Prologue Extractor's reverse proxy (see
// vite.config.ts), so every call still traverses the proxy and is still captured as an HTTP command — but the
// browser never makes a cross-origin request, and so never sends a preflight.
//
// Calling the proxy directly from the browser does not work: a POST carrying JSON triggers an OPTIONS preflight,
// the proxy answers it with 404 rather than passing it to the API, and every write is blocked while reads carry
// on working. Going through Vite removes that whole class of problem rather than negotiating with it.
const baseUrl = '';

const noContent = 204;

const readProblem = async (response: Response): Promise<ProblemDetails> => {
    const fallback: ProblemDetails = {
        title: `The request failed with status ${response.status}`,
        detail: response.statusText,
        status: response.status
    };

    const body = await response.text().catch(() => '');

    if (body.length === 0) {
        return fallback;
    }

    try {
        const problem = JSON.parse(body) as ProblemDetails;
        return problem.title === undefined && problem.detail === undefined
            ? fallback
            : { ...problem, status: problem.status ?? response.status };
    } catch {
        // The body was not problem details JSON — fall back to what the status line tells us.
        return fallback;
    }
};

const readBody = async <TResponse>(response: Response): Promise<TResponse> => {
    if (response.status === noContent) {
        return undefined as TResponse;
    }

    const body = await response.text();
    return (body.length === 0 ? undefined : JSON.parse(body)) as TResponse;
};

const request = async <TResponse>(path: string, init: RequestInit): Promise<TResponse> => {
    const response = await fetch(`${baseUrl}${path}`, init);

    if (!response.ok) {
        throw new RequestFailed(await readProblem(response));
    }

    return await readBody<TResponse>(response);
};

const asJson = (body: unknown): RequestInit => ({
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body ?? {})
});

/**
 * The single place the app talks HTTP, so error handling and the proxy base URL live in one spot.
 */
export const apiClient = {
    /**
     * Gets a resource.
     * @param path The path, relative to the API base URL.
     * @returns The deserialized response.
     */
    get: <TResponse>(path: string): Promise<TResponse> => request<TResponse>(path, { method: 'GET' }),

    /**
     * Posts a resource.
     * @param path The path, relative to the API base URL.
     * @param body The body to send.
     * @returns The deserialized response.
     */
    post: <TResponse>(path: string, body?: unknown): Promise<TResponse> =>
        request<TResponse>(path, { method: 'POST', ...asJson(body) }),

    /**
     * Removes a resource.
     * @param path The path, relative to the API base URL.
     */
    remove: (path: string): Promise<void> => request<void>(path, { method: 'DELETE' })
};
