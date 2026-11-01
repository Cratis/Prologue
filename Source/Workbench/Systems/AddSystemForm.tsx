// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { FormEvent, useState } from 'react';
import { Button, Field } from '../Components';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { AddSystem } from './Adding/AddSystem';
import { LocateSystemCode } from './Locating/LocateSystemCode';
import { LocationFields } from './LocationFields';
import { emptyDraft, useLocationDraft } from './useSystemDraft';

export interface AddSystemFormProps {
    readonly prologue: Guid;
    readonly onClose: () => void;
}

/** Adds a system: its name, which becomes its service name, and where its code lives. */
export const AddSystemForm = ({ prologue, onClose }: AddSystemFormProps) => {
    const { run, isBusy } = useCommandRunner();
    const { draft, change, reset } = useLocationDraft(emptyDraft);
    const [name, setName] = useState('');

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const add = new AddSystem();
        add.prologue = prologue;
        add.id = Guid.create();
        add.name = name;

        if (!await run('add-system', add, `${name.trim()} added`)) return;

        if (draft.repository.trim().length > 0) {
            const locate = new LocateSystemCode();
            locate.prologue = prologue;
            locate.id = add.id;
            locate.host = draft.host;
            locate.repository = draft.repository;
            locate.branch = draft.branch;
            locate.subPath = draft.subPath;
            await run('locate-system', locate);
        }

        setName('');
        reset();
        onClose();
    };

    return (
        <form className="addsys" onSubmit={submit}>
            <Field label="System name" hint="Its service name in the configuration">
                <input value={name} placeholder="Fines" onChange={event => setName(event.target.value)} />
            </Field>
            <LocationFields draft={draft} change={change} />
            <div className="row form-actions">
                <Button type="submit" variant="primary" small disabled={isBusy('add-system')}>Add</Button>
                <Button small onClick={onClose}>Cancel</Button>
            </div>
        </form>
    );
};
