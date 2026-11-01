// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { FormEvent, useState } from 'react';
import { Button, Field, Panel } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { usePrologues } from '../../Shell/shellState';
import { DefinePrologue } from './DefinePrologue';

/** Defines a prologue. Shown on the Systems step when there is none, and when the user asks for a new one. */
export const NewPrologue = () => {
    const { run, isBusy } = useCommandRunner();
    const { hasPrologue, selectPrologue, setCreating } = usePrologues();
    const [name, setName] = useState('');

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const command = new DefinePrologue();
        command.id = Guid.create();
        command.name = name;
        if (await run('define-prologue', command, `${name.trim()} defined`)) {
            selectPrologue(String(command.id));
            setName('');
        }
    };

    return (
        <Panel title="Define a prologue" aside="The work of capturing and interpreting one existing system">
            <form className="grid new-prologue" onSubmit={submit}>
                <Field label="Name" hint="For example, the name of the product the systems make up.">
                    <input value={name} placeholder="Library" autoFocus onChange={event => setName(event.target.value)} />
                </Field>
                <div className="row">
                    <Button type="submit" variant="primary" disabled={isBusy('define-prologue')}>Define prologue</Button>
                    {hasPrologue && <Button onClick={() => setCreating(false)}>Cancel</Button>}
                </div>
            </form>
        </Panel>
    );
};
