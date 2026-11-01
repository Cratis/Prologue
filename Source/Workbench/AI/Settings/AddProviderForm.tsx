// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AIProviderType } from '@cratis/ai';
import { Button, Field } from '../../Components';
import { concurrencyExplanation } from './explanations';
import { useAddProviderForm } from './useAddProviderForm';
import { vendors } from './vendors';

export interface AddProviderFormProps {
    readonly onClose: () => void;
}

/** The form that adds a provider; what it asks for depends on the vendor chosen. */
export const AddProviderForm = ({ onClose }: AddProviderFormProps) => {
    const form = useAddProviderForm(onClose);
    const { vendor, draft } = form;

    return (
        <form className="addprov" onSubmit={event => void form.submit(event)}>
            <Field label="Vendor">
                <select value={draft.type} onChange={event => form.chooseVendor(Number(event.target.value) as AIProviderType)}>
                    {vendors.map(entry => <option key={entry.type} value={entry.type}>{entry.label}</option>)}
                </select>
            </Field>
            <Field label="Name">
                <input value={draft.name} placeholder="A recognizable name for the provider" onChange={event => form.change({ name: event.target.value })} />
            </Field>
            {vendor.endpoint && (
                <Field label="Endpoint">
                    <input value={draft.endpoint} placeholder={vendor.endpoint.placeholder} onChange={event => form.change({ endpoint: event.target.value })} />
                </Field>
            )}
            {vendor.apiKey && (
                <Field label="API key (stored as a secret)">
                    <input type="password" autoComplete="off" value={draft.apiKey} placeholder={vendor.apiKey.placeholder} onChange={event => form.change({ apiKey: event.target.value })} />
                </Field>
            )}
            <Field label="Max concurrent jobs">
                <input type="number" min={0} value={draft.maxConcurrentJobs} onChange={event => form.change({ maxConcurrentJobs: Math.max(0, Number(event.target.value) || 0) })} />
            </Field>
            <p className="muted note">{concurrencyExplanation}</p>
            {vendor.note && <p className="muted note">{vendor.note}</p>}
            <div className="row actions">
                <Button type="submit" variant="primary" small disabled={!form.canAdd || form.adding}>Add</Button>
                <Button small onClick={onClose}>Cancel</Button>
            </div>
        </form>
    );
};
