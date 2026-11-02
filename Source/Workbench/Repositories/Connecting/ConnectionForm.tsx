// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { FormEvent, useState } from 'react';
import { Button, Field } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { hostInfo } from '../../Systems/hosts';
import { RepositoryHost } from '../../Systems/RepositoryHost';
import { RepositoryConnection } from '../RepositoryConnection';
import { ConnectAzureDevOps } from './ConnectAzureDevOps';
import { ConnectGitHub } from './ConnectGitHub';
import { ConnectGitHubApp } from './ConnectGitHubApp';
import { ConnectGitLab } from './ConnectGitLab';
import { DisconnectRepositoryHost } from './DisconnectRepositoryHost';

export interface ConnectionFormProps {
    readonly host: RepositoryHost;
    readonly connection?: RepositoryConnection;
    readonly onClose: () => void;
}

const defaultApi = 'https://api.github.com';

const defaultUrls: Partial<Record<RepositoryHost, string>> = {
    [RepositoryHost.gitHub]: 'https://github.com',
    [RepositoryHost.azureDevOps]: 'https://dev.azure.com/',
    [RepositoryHost.gitLab]: 'https://gitlab.com'
};

/** Connects a host with the address of its server and the name of the secret holding the access token, or disconnects it. */
export const ConnectionForm = ({ host, connection, onClose }: ConnectionFormProps) => {
    const { run, isBusy } = useCommandRunner();
    const info = hostInfo(host);
    const [url, setUrl] = useState(connection?.url ?? defaultUrls[host] ?? '');
    const [secret, setSecret] = useState(connection?.secret ?? '');
    const [installation, setInstallation] = useState(connection?.installation ? String(connection.installation) : '');
    const [appId, setAppId] = useState(connection?.app?.app ? String(connection.app.app) : '');
    const [privateKey, setPrivateKey] = useState(connection?.app?.privateKey ?? '');
    const [api, setApi] = useState(connection?.app && connection.app.api !== defaultApi ? connection.app.api : '');
    const useApp = host === RepositoryHost.gitHub && installation.trim().length > 0 && secret.trim().length === 0;

    const connect = () => {
        if (useApp) {
            const command = new ConnectGitHubApp();
            command.url = url;
            command.installation = Number(installation);
            command.app = Number(appId);
            command.privateKey = privateKey;
            command.api = api;
            return run('connect', command, 'GitHub connected');
        }

        const command = host === RepositoryHost.azureDevOps ? new ConnectAzureDevOps() : host === RepositoryHost.gitLab ? new ConnectGitLab() : new ConnectGitHub();
        if (command instanceof ConnectAzureDevOps) command.organization = url;
        else command.url = url;
        command.secret = secret;
        return run('connect', command, `${info.label} connected`);
    };

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        if (await connect()) onClose();
    };

    const disconnect = async () => {
        const command = new DisconnectRepositoryHost();
        command.host = host;
        if (await run('disconnect', command, `${info.label} disconnected`)) onClose();
    };

    return (
        <form className="addsys" onSubmit={submit}>
            <Field label={host === RepositoryHost.azureDevOps ? 'Organization address' : 'Server address'}>
                <input value={url} onChange={event => setUrl(event.target.value)} />
            </Field>
            <Field label="Access token, by secret name" hint="The name of the secret that holds the token. Resolved from the Receiver's configuration or environment when it clones; the token is never stored or shown.">
                <input value={secret} placeholder={host === RepositoryHost.azureDevOps ? 'ado/prologue-read' : `${info.label.toLowerCase().replace(' ', '')}/prologue-read`} onChange={event => setSecret(event.target.value)} />
            </Field>
            {host === RepositoryHost.gitHub && (
                <>
                    <Field label="Or a GitHub App installation" hint="Leave the access token's secret name empty to clone through an app installation instead. The Receiver mints a short-lived token for it whenever it clones.">
                        <input inputMode="numeric" placeholder="Installation id" value={installation} onChange={event => setInstallation(event.target.value.replace(/\D/g, ''))} />
                    </Field>
                    {installation.trim().length > 0 && (
                        <>
                            <Field label="App id">
                                <input inputMode="numeric" value={appId} onChange={event => setAppId(event.target.value.replace(/\D/g, ''))} />
                            </Field>
                            <Field label="Private key, by secret name" hint="The name of the secret that holds the app's PEM private key. The key is never stored or shown.">
                                <input value={privateKey} placeholder="github/prologue-app-key" onChange={event => setPrivateKey(event.target.value)} />
                            </Field>
                            <Field label="API address (GitHub Enterprise)" hint={`Leave empty for github.com, whose API is ${defaultApi}.`}>
                                <input value={api} placeholder={defaultApi} onChange={event => setApi(event.target.value)} />
                            </Field>
                        </>
                    )}
                </>
            )}
            <div className="row form-actions">
                <Button type="submit" variant="primary" small disabled={isBusy('connect')}>{connection ? 'Save' : 'Connect'}</Button>
                {connection && <Button small disabled={isBusy('disconnect')} onClick={disconnect}>Disconnect</Button>}
                <Button small onClick={onClose}>Cancel</Button>
            </div>
        </form>
    );
};
