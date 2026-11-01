// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

export interface FieldProps {
    readonly label: string;
    readonly hint?: string;

    /** The input, select or textarea the label belongs to. */
    readonly children: ReactNode;
}

export const Field = ({ label, hint, children }: FieldProps) => (
    <label className="field">
        {label}
        {children}
        {hint && <span className="hint">{hint}</span>}
    </label>
);
