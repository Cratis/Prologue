// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, Chip, Field, Panel, ProviderMark } from '../Components';
import { hostInfo, hosts } from '../Systems/hosts';
import { RepositoryHost } from '../Systems/RepositoryHost';
import { ConnectionForm } from './Connecting/ConnectionForm';
import { GetCloneSettings } from './CloneSettings';
import { ObserveRepositoryConnections, RepositoryConnection } from './RepositoryConnection';

const detailOf = (connection: RepositoryConnection): string =>
    connection.installation
        ? `${connection.url} · GitHub App installation ${connection.installation}`
        : `${connection.url} · token from secret ${connection.secret}`;

/** The hosts systems can live on: connect a host with the name of its token secret, or analyze a local folder with no connection at all. */
export const RepositoryConnectionsPanel = () => {
    const [connections] = ObserveRepositoryConnections.use();
    const [settings] = GetCloneSettings.use();
    const [managing, setManaging] = useState<RepositoryHost | undefined>();

    const connectionOf = (host: RepositoryHost) => connections.data.find(connection => connection.host === host);

    return (
        <Panel title="Repository connections" aside="Cloned read-only, inside your network">
            <div>
                {hosts.map(info => {
                    const connection = connectionOf(info.host);
                    const local = info.host === RepositoryHost.localFolder;
                    return (
                        <div key={info.host} className="conn">
                            <ProviderMark abbreviation={info.abbreviation} tone={info.tone} label={info.label} />
                            <span className="grow">
                                <b>{info.label}</b>
                                <small>{local ? 'Analyze code already on this machine' : connection ? detailOf(connection) : 'Not connected'}</small>
                            </span>
                            {local && <Chip>No connection needed</Chip>}
                            {!local && connection && <Chip variant="ok">Connected</Chip>}
                            {!local && <Button small onClick={() => setManaging(info.host)}>{connection ? 'Manage' : 'Connect'}</Button>}
                        </div>
                    );
                })}
            </div>
            {managing !== undefined && (
                <ConnectionForm key={managing} host={managing} connection={connectionOf(managing)} onClose={() => setManaging(undefined)} />
            )}
            <Field label="Clone into" hint="Set with Prologue:Repositories:CloneDirectory in the Receiver's configuration.">
                <input readOnly value={settings.data?.cloneDirectory ?? ''} />
            </Field>
            <p className="muted small-text note">A public repository can be cloned without a connection. {hostInfo(RepositoryHost.localFolder).label} systems are read where they are.</p>
        </Panel>
    );
};
