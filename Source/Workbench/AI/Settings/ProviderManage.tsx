// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AIProvider } from '@cratis/ai';
import { Button, Field, Toggle } from '../../Components';
import { concurrencyExplanation, networkExplanation, usageCapacityExplanation } from './explanations';
import { useProviderManage } from './useProviderManage';
import { vendorOf } from './vendors';

export interface ProviderManageProps {
    readonly provider: AIProvider;
    readonly inside: boolean;
}

/** The panel behind a provider's Manage button. Credentials are never shown here: a stored key cannot be read back. */
export const ProviderManage = ({ provider, inside }: ProviderManageProps) => {
    const manage = useProviderManage(provider);
    const signIn = vendorOf(provider.type)?.deviceSignIn;

    return (
        <div className="manage">
            <div className="manage-grid">
                <Field label="Max concurrent jobs" hint={concurrencyExplanation}>
                    <span className="row">
                        <input type="number" min={0} value={manage.concurrency} onChange={event => manage.setConcurrency(Math.max(0, Number(event.target.value) || 0))} />
                        <Button small disabled={manage.isBusy('concurrency')} onClick={() => void manage.saveConcurrency()}>Save</Button>
                    </span>
                </Field>
                <Field label="Usage capacity (tokens)" hint={usageCapacityExplanation}>
                    <span className="row">
                        <input type="number" min={0} value={manage.capacity} onChange={event => manage.setCapacity(Math.max(0, Number(event.target.value) || 0))} />
                        <Button small disabled={manage.isBusy('capacity')} onClick={() => void manage.saveCapacity()}>Save</Button>
                    </span>
                </Field>
            </div>
            <Toggle label="Runs inside my network" checked={inside} onChange={checked => void manage.markInside(checked)} />
            <p className="muted note">{networkExplanation}</p>
            <div className="row actions">
                {signIn && provider.isSubscription && (
                    <Button small disabled={manage.isBusy('disconnect')} onClick={() => void manage.disconnect()}>Disconnect {signIn.account}</Button>
                )}
                {manage.confirmingRemove ? (
                    <>
                        <span className="muted">Agents using this provider stop running until pointed elsewhere.</span>
                        <Button small variant="primary" disabled={manage.isBusy('remove')} onClick={() => void manage.remove()}>Remove {provider.name}</Button>
                        <Button small variant="ghost" onClick={() => manage.setConfirmingRemove(false)}>Keep</Button>
                    </>
                ) : (
                    <Button small variant="ghost" onClick={() => manage.setConfirmingRemove(true)}>Remove</Button>
                )}
            </div>
        </div>
    );
};
