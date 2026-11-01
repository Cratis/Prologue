// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { AIProvider, PendingSubscriptionSignIn } from '@cratis/ai';
import { Button } from '../../Components';
import { useCommandRunner } from '../../Shell/useCommandRunner';
import { ProviderCapacity } from './ProviderCapacity';
import { ProviderManage } from './ProviderManage';
import { isHttpsUrl } from './safeLink';
import { vendorOf } from './vendors';

export interface ProviderRowProps {
    readonly provider: AIProvider;
    readonly inside: boolean;

    /** The device sign-in waiting for its code to be entered, if one is. */
    readonly pendingSignIn?: PendingSubscriptionSignIn;
}

const connectionLine = (provider: AIProvider, pending: boolean): string | undefined => {
    if (!vendorOf(provider.type)?.deviceSignIn) return undefined;
    if (pending) return undefined;
    return provider.isSubscription ? 'Connected' : 'Not connected';
};

/** One configured provider: its mark, name, where it runs, how much is left and the buttons that act on it. */
export const ProviderRow = ({ provider, inside, pendingSignIn }: ProviderRowProps) => {
    const [managing, setManaging] = useState(false);
    const { run, isBusy } = useCommandRunner();
    const vendor = vendorOf(provider.type);
    const connection = connectionLine(provider, pendingSignIn !== undefined);
    const needsConnect = vendor?.deviceSignIn !== undefined && !provider.isSubscription && pendingSignIn === undefined;
    const concurrency = provider.maxConcurrentJobs ? `${provider.maxConcurrentJobs} concurrent` : 'no concurrency limit';

    const connect = () => {
        if (!vendor?.deviceSignIn) return Promise.resolve(false);
        const command = new vendor.deviceSignIn.start();
        command.provider = provider.id;
        return run(`connect-${provider.id}`, command, `Started signing ${provider.name} in - enter the code shown on its row`);
    };

    return (
        <div className="prow-wrap">
            <div className="prow">
                <span className={inside ? 'vmark local' : 'vmark'} aria-hidden="true">{vendor?.mark ?? '?'}</span>
                <span>
                    <b>{provider.name}</b> <span className="muted small-text">{vendor?.label}</span>
                    <small>
                        {inside && <span className="inside">In your network · </span>}
                        {provider.endpoint && <span className="mono">{provider.endpoint} · </span>}
                        {connection && `${connection} · `}{concurrency}
                    </small>
                    {provider.rateLimitedUntil && <small className="caution">Rate limited until {provider.rateLimitedUntil.toLocaleString()}</small>}
                    {pendingSignIn && (
                        <small>Enter code <b className="mono">{pendingSignIn.userCode}</b> at {isHttpsUrl(pendingSignIn.verificationUrl)
                            ? <a href={pendingSignIn.verificationUrl} target="_blank" rel="noreferrer">{pendingSignIn.verificationUrl}</a>
                            : <span className="mono">{pendingSignIn.verificationUrl}</span>}</small>
                    )}
                    <ProviderCapacity provider={provider} />
                </span>
                <span className="row">
                    {needsConnect && <Button small disabled={isBusy(`connect-${provider.id}`)} onClick={() => void connect()}>Connect</Button>}
                    <Button small aria-expanded={managing} onClick={() => setManaging(open => !open)}>Manage</Button>
                </span>
            </div>
            {managing && <ProviderManage provider={provider} inside={inside} />}
        </div>
    );
};
