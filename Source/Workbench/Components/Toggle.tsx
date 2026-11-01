// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface ToggleProps {
    readonly label: string;
    readonly checked: boolean;
    readonly onChange: (checked: boolean) => void;

    /** Keeps the label for assistive technology only, when the surrounding layout already names the control. */
    readonly hideLabel?: boolean;
}

export const Toggle = ({ label, checked, onChange, hideLabel = false }: ToggleProps) => (
    <label className="toggle">
        <span className="track">
            <input type="checkbox" role="switch" checked={checked} onChange={event => onChange(event.target.checked)} />
            <span aria-hidden="true" />
        </span>
        <span className={hideLabel ? 'visually-hidden' : undefined}>{label}</span>
    </label>
);
