// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { FormEvent, useState } from 'react';
import { Button, Field } from '../Components';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { PrologueSystem } from './PrologueSystem';
import { RenameSystem } from './Renaming/RenameSystem';

export interface RenameFormProps {
    readonly prologue: Guid;
    readonly system: PrologueSystem;
    readonly onClose: () => void;
}

/** Renames a system. The name is its service name, so the generated configuration follows. */
export const RenameForm = ({ prologue, system, onClose }: RenameFormProps) => {
    const { run, isBusy } = useCommandRunner();
    const [name, setName] = useState(system.name);

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const rename = new RenameSystem();
        rename.prologue = prologue;
        rename.id = system.id;
        rename.name = name;
        if (await run('rename-system', rename, 'System renamed')) onClose();
    };

    return (
        <form className="addsys" onSubmit={submit}>
            <Field label={`Rename ${system.name}`} hint="Its service name in the configuration">
                <input value={name} onChange={event => setName(event.target.value)} />
            </Field>
            <div className="row form-actions">
                <Button type="submit" variant="primary" small disabled={isBusy('rename-system')}>Rename</Button>
                <Button small onClick={onClose}>Cancel</Button>
            </div>
        </form>
    );
};
