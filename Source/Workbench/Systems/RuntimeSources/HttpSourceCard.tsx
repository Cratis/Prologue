// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useState } from 'react';
import { Field } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { formatList, parseList, sameList } from '../lists';
import { PrologueSystem } from '../PrologueSystem';
import { RuntimeSourceCard } from './RuntimeSourceCard';
import { SetHttpClusters } from './SetHttpClusters';

/** The HTTP clusters Capture records commands from for a system. */
export const HttpSourceCard = ({ prologue, system }: { prologue: Guid; system: PrologueSystem }) => {
    const { run, isBusy } = useCommandRunner();
    const [text, setText] = useState(formatList(system.httpClusters));

    const save = () => {
        const command = new SetHttpClusters();
        command.prologue = prologue;
        command.id = system.id;
        command.clusters = parseList(text);
        void run('http-clusters', command, 'HTTP clusters saved');
    };

    return (
        <RuntimeSourceCard title="HTTP commands" changed={!sameList(parseList(text), system.httpClusters)} busy={isBusy('http-clusters')} onSave={save}>
            <Field label="Clusters" hint="Where the system is reached, comma separated. Only POST, PUT and DELETE are recorded.">
                <input value={text} placeholder="library-core" onChange={event => setText(event.target.value)} />
            </Field>
        </RuntimeSourceCard>
    );
};
