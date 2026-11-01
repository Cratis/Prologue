// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { FormEvent } from 'react';
import { Button } from '../Components';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { LocateSystemCode } from './Locating/LocateSystemCode';
import { LocationFields } from './LocationFields';
import { PrologueSystem } from './PrologueSystem';
import { draftOf, useLocationDraft } from './useSystemDraft';

export interface EditLocationFormProps {
    readonly prologue: Guid;
    readonly system: PrologueSystem;
    readonly onClose: () => void;
}

/** Says where an existing system's code lives, or moves it. Systems may share a repository by pointing at different sub paths. */
export const EditLocationForm = ({ prologue, system, onClose }: EditLocationFormProps) => {
    const { run, isBusy } = useCommandRunner();
    const { draft, change } = useLocationDraft(draftOf(system.location));

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const locate = new LocateSystemCode();
        locate.prologue = prologue;
        locate.id = system.id;
        locate.host = draft.host;
        locate.repository = draft.repository;
        locate.branch = draft.branch;
        locate.subPath = draft.subPath;
        if (await run('locate-system', locate, `Location of ${system.name} saved`)) onClose();
    };

    return (
        <form className="addsys" onSubmit={submit}>
            <LocationFields draft={draft} change={change} />
            <div className="row form-actions">
                <Button type="submit" variant="primary" small disabled={isBusy('locate-system')}>Save location</Button>
                <Button small onClick={onClose}>Cancel</Button>
            </div>
        </form>
    );
};
