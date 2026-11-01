// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useState } from 'react';
import { Button } from '../Components';
import { CloneSystemRepository } from '../Repositories/Cloning/CloneSystemRepository';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { PrologueSystem } from './PrologueSystem';
import { RemoveSystem } from './Removing/RemoveSystem';

export interface SystemRowActionsProps {
    readonly prologue: Guid;
    readonly system: PrologueSystem;
    readonly cloned: boolean;
    readonly onRename: () => void;
    readonly onLocate: () => void;
}

/** What can be done to one system: clone or pull its code again, rename it, move its code and remove it. */
export const SystemRowActions = ({ prologue, system, cloned, onRename, onLocate }: SystemRowActionsProps) => {
    const { run, isBusy } = useCommandRunner();
    const [confirming, setConfirming] = useState(false);

    const clone = () => {
        const command = new CloneSystemRepository();
        command.prologue = prologue;
        command.id = system.id;
        void run(`clone-${system.id}`, command, system.location?.host === 3 ? 'Folder read' : 'Repository is up to date');
    };

    const remove = async () => {
        const command = new RemoveSystem();
        command.prologue = prologue;
        command.id = system.id;
        await run(`remove-${system.id}`, command, `${system.name} removed`);
        setConfirming(false);
    };

    return (
        <div className="row actions">
            {system.location && (
                <Button small disabled={isBusy(`clone-${system.id}`)} onClick={clone}>{cloned ? 'Pull again' : 'Clone'}</Button>
            )}
            <Button small variant="ghost" onClick={onLocate}>{system.location ? 'Move code' : 'Locate code'}</Button>
            <Button small variant="ghost" onClick={onRename}>Rename</Button>
            {confirming
                ? <Button small disabled={isBusy(`remove-${system.id}`)} onClick={remove}>Confirm remove</Button>
                : <Button small variant="ghost" onClick={() => setConfirming(true)}>Remove</Button>}
        </div>
    );
};
