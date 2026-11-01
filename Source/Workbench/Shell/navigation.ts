// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** How a navigation is recorded in the history. */
export interface NavigateOptions {
    /** Replace the current history entry instead of adding one, so Back skips a redirect. */
    readonly replace?: boolean;
}

/** The part of the History API a navigation needs. */
export interface HistoryLike {
    readonly pathname: () => string;
    readonly pushState: (data: unknown, unused: string, url: string) => void;
    readonly replaceState: (data: unknown, unused: string, url: string) => void;
}

/**
 * Records a navigation in the history.
 * @returns True when the location changed; navigating to where it already is does nothing.
 */
export const commitNavigation = (history: HistoryLike, target: string, options?: NavigateOptions): boolean => {
    if (target === history.pathname()) return false;
    if (options?.replace) history.replaceState({}, '', target);
    else history.pushState({}, '', target);
    return true;
};
