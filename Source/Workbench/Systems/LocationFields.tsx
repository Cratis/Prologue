// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Field } from '../Components';
import { hostInfo, hosts } from './hosts';
import { LocationDraft } from './useSystemDraft';

export interface LocationFieldsProps {
    readonly draft: LocationDraft;
    readonly change: <K extends keyof LocationDraft>(field: K, value: LocationDraft[K]) => void;
}

/** The four fields that say where a system's code lives: host, repository or folder, branch and sub path. */
export const LocationFields = ({ draft, change }: LocationFieldsProps) => {
    const info = hostInfo(draft.host);
    const local = info.label === 'Local folder';

    return (
        <>
            <Field label="Repository host">
                <select value={draft.host} onChange={event => change('host', Number(event.target.value))}>
                    {hosts.map(option => <option key={option.host} value={option.host}>{option.label}</option>)}
                </select>
            </Field>
            <Field label={info.repositoryLabel}>
                <input value={draft.repository} placeholder={info.repositoryPlaceholder} onChange={event => change('repository', event.target.value)} />
            </Field>
            {!local && (
                <Field label="Branch">
                    <input value={draft.branch} onChange={event => change('branch', event.target.value)} />
                </Field>
            )}
            <Field label="Sub path" hint="Empty when the system is the whole repository">
                <input value={draft.subPath} placeholder="services/fines" onChange={event => change('subPath', event.target.value)} />
            </Field>
        </>
    );
};
