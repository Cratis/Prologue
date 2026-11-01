// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, Chip, DataColumn, DataTable, Panel, ProviderMark } from '../Components';
import { CloneStatusChip } from '../Repositories/Cloning/CloneStatusChip';
import { ObserveSystemClones, SystemClone } from '../Repositories/Cloning/SystemClone';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { AddSystemForm } from './AddSystemForm';
import { EditLocationForm } from './EditLocationForm';
import { hostInfo } from './hosts';
import { PrologueSystem } from './PrologueSystem';
import { RenameForm } from './RenameForm';
import { SystemRowActions } from './SystemRowActions';

type Editing = { readonly kind: 'add' } | { readonly kind: 'rename' | 'locate'; readonly system: PrologueSystem };

const captureChips = (system: PrologueSystem) => {
    const chips = [
        system.httpClusters.length > 0 && <Chip key="http" variant="http">HTTP</Chip>,
        system.databases.length > 0 && <Chip key="db" variant="db">{system.databases.map(database => database.engine === 0 ? 'SQL Server' : 'PostgreSQL').join(', ')}</Chip>,
        system.telemetryServices.length > 0 && <Chip key="otel" variant="otel">OpenTelemetry</Chip>
    ].filter(Boolean);
    return chips.length > 0 ? <span className="row chips">{chips}</span> : <span className="muted small-text">Not captured</span>;
};

/** The systems of the prologue: a table of where each one's code lives and what is captured from it, and the forms that change them. */
export const SystemsPanel = ({ prologue }: { prologue: PrologueDefinition }) => {
    const [clones] = ObserveSystemClones.use();
    const [editing, setEditing] = useState<Editing | undefined>();

    const cloneOf = (system: PrologueSystem): SystemClone | undefined => clones.data.find(clone => String(clone.id) === String(system.id));
    const close = () => setEditing(undefined);

    const columns: DataColumn<PrologueSystem>[] = [
        { key: 'name', header: 'System', render: system => <b>{system.name}</b> },
        {
            key: 'repository', header: 'Repository', render: system => {
                if (!system.location) return <span className="muted small-text">Not located yet</span>;
                const info = hostInfo(system.location.host);
                return (
                    <span className="row nowrap">
                        <ProviderMark abbreviation={info.abbreviation} tone={info.tone} label={info.label} />
                        <span>
                            <span className="mono nowrap">{system.location.repository}</span>
                            {system.location.host !== 3 && <span className="muted mono block small-text">{system.location.branch}</span>}
                        </span>
                    </span>
                );
            }
        },
        { key: 'path', header: 'Path', render: system => <span className="mono">{system.location?.subPath || (system.location ? '/' : '')}</span> },
        { key: 'capture', header: 'Capture', render: captureChips },
        {
            key: 'code', header: 'Code', render: system => (
                <CloneStatusChip located={system.location !== undefined} host={system.location?.host} clone={cloneOf(system)} />
            )
        },
        {
            key: 'actions', header: '', render: system => (
                <SystemRowActions
                    prologue={prologue.id}
                    system={system}
                    cloned={cloneOf(system) !== undefined}
                    onRename={() => setEditing({ kind: 'rename', system })}
                    onLocate={() => setEditing({ kind: 'locate', system })} />
            )
        }
    ];

    return (
        <Panel title="Systems" aside="A system can be its own repository or a sub path in a monorepo">
            <DataTable
                caption="Systems"
                columns={columns}
                rows={prologue.systems}
                rowKey={system => String(system.id)}
                emptyTitle="No systems yet. Add the first one." />
            <div className="row panel-actions">
                <Button small onClick={() => setEditing({ kind: 'add' })}>Add system</Button>
                <span className="muted small-text">Systems in one monorepo each point at their own sub path.</span>
            </div>
            {editing?.kind === 'add' && <AddSystemForm prologue={prologue.id} onClose={close} />}
            {editing?.kind === 'rename' && <RenameForm key={String(editing.system.id)} prologue={prologue.id} system={editing.system} onClose={close} />}
            {editing?.kind === 'locate' && <EditLocationForm key={String(editing.system.id)} prologue={prologue.id} system={editing.system} onClose={close} />}
        </Panel>
    );
};
