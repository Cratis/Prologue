// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ProblemDetails } from './ProblemDetails';
import { RequestFailed } from './RequestFailed';

// The base URL points at the Prologue Extractor's reverse proxy, not at the API directly. Every XHR the
// SPA makes has to traverse the proxy so the Extractor captures it as an HTTP command.
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080';

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
