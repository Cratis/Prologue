// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface ToggleProps {
    readonly label: string;
    readonly checked: boolean;
    readonly onChange: (checked: boolean) => void;
}

export const Toggle = ({ label, checked, onChange }: ToggleProps) => (
    <label className="toggle">
        <span className="track">
            <input type="checkbox" role="switch" checked={checked} onChange={event => onChange(event.target.checked)} />
            <span aria-hidden="true" />
        </span>
        <span>{label}</span>
    </label>
);
