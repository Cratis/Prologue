// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useState } from 'react';
import { Field } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { DatabaseEngine } from '../DatabaseEngine';
import { DatabaseSource } from '../DatabaseSource';
import { formatList, parseList, sameList } from '../lists';
import { PrologueSystem } from '../PrologueSystem';
import { RuntimeSourceCard } from './RuntimeSourceCard';
import { SetDatabaseSources } from './SetDatabaseSources';

/** A database whose transaction log is read for a system. Only the name of the secret holding the connection string is entered. */
export const DatabaseSourceCard = ({ prologue, system }: { prologue: Guid; system: PrologueSystem }) => {
    const { run, isBusy } = useCommandRunner();
    const current = system.databases[0];
    const [engine, setEngine] = useState<DatabaseEngine>(current?.engine ?? DatabaseEngine.postgreSql);
    const [secret, setSecret] = useState(current?.connectionString ?? '');
    const [tables, setTables] = useState(formatList(current?.tables ?? []));

    const changed = (current?.engine ?? engine) !== engine
        || (current?.connectionString ?? '') !== secret.trim()
        || !sameList(parseList(tables), current?.tables ?? []);

    const save = () => {
        const command = new SetDatabaseSources();
        command.prologue = prologue;
        command.id = system.id;
        command.sources = [];
        if (secret.trim().length > 0) {
            const source = new DatabaseSource();
            source.engine = engine;
            source.connectionString = secret;
            source.tables = parseList(tables);
            command.sources = [source];
        }
        void run('database-sources', command, 'Database saved');
    };

    return (
        <RuntimeSourceCard title="Database changes" changed={changed} busy={isBusy('database-sources')} onSave={save}>
            <Field label="Engine">
                <select value={engine} onChange={event => setEngine(Number(event.target.value))}>
                    <option value={DatabaseEngine.postgreSql}>PostgreSQL · logical replication</option>
                    <option value={DatabaseEngine.sqlServer}>SQL Server · change data capture</option>
                </select>
            </Field>
            <Field label="Connection string, by secret name" hint="The name of the secret, such as library-db/connection. The connection string itself is never entered here.">
                <input value={secret} placeholder="library-db/connection" onChange={event => setSecret(event.target.value)} />
            </Field>
            <Field label="Tables to watch" hint="Comma separated. Empty watches every table.">
                <input value={tables} placeholder="Loans, Members" onChange={event => setTables(event.target.value)} />
            </Field>
        </RuntimeSourceCard>
    );
};
