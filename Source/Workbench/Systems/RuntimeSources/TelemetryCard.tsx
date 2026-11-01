// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useState } from 'react';
import { Field } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { formatList, parseList, sameList } from '../lists';
import { PrologueSystem } from '../PrologueSystem';
import { RuntimeSourceCard } from './RuntimeSourceCard';
import { SetTelemetry } from './SetTelemetry';

/** The OpenTelemetry services captured for a system and the attribute keys whose values may be kept. */
export const TelemetryCard = ({ prologue, system }: { prologue: Guid; system: PrologueSystem }) => {
    const { run, isBusy } = useCommandRunner();
    const [services, setServices] = useState(formatList(system.telemetryServices));
    const [keys, setKeys] = useState(formatList(system.attributeKeys));

    const changed = !sameList(parseList(services), system.telemetryServices) || !sameList(parseList(keys), system.attributeKeys);

    const save = () => {
        const command = new SetTelemetry();
        command.prologue = prologue;
        command.id = system.id;
        command.services = parseList(services);
        command.attributeKeys = parseList(keys);
        void run('telemetry', command, 'Telemetry saved');
    };

    return (
        <RuntimeSourceCard title="OpenTelemetry" changed={changed} busy={isBusy('telemetry')} onSave={save}>
            <Field label="Services" hint="The service names the system reports as, comma separated.">
                <input value={services} placeholder="library-core" onChange={event => setServices(event.target.value)} />
            </Field>
            <Field label="Attribute values kept" hint="Keys are always visible. Values are kept only for the keys listed here.">
                <input value={keys} placeholder="http.route, library.isbn" onChange={event => setKeys(event.target.value)} />
            </Field>
        </RuntimeSourceCard>
    );
};
