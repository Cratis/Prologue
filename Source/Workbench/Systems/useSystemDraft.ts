// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { hosts } from './hosts';
import { RepositoryHost } from './RepositoryHost';
import { SystemCodeLocation } from './SystemCodeLocation';

/** What the user has typed about where a system's code lives. */
export interface LocationDraft {
    readonly host: RepositoryHost;
    readonly repository: string;
    readonly branch: string;
    readonly subPath: string;
}

export const emptyDraft: LocationDraft = { host: hosts[0].host, repository: '', branch: 'main', subPath: '' };

export const draftOf = (location?: SystemCodeLocation): LocationDraft => location
    ? { host: location.host, repository: location.repository, branch: location.branch, subPath: location.subPath }
    : emptyDraft;

/** The draft of a system's code location and the means to change one field of it. */
export const useLocationDraft = (initial: LocationDraft = emptyDraft) => {
    const [draft, setDraft] = useState(initial);
    const change = <K extends keyof LocationDraft>(field: K, value: LocationDraft[K]) => setDraft(previous => ({ ...previous, [field]: value }));
    return { draft, change, reset: () => setDraft(initial) };
};
