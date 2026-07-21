// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ReactNode } from 'react';

/**
 * The props for {@link Field}.
 */
export interface FieldProps {
    /** The label shown above the control. */
    label: string;

    /** The input or select the label belongs to. */
    children: ReactNode;
}

/**
 * A labeled form control.
 * @param props The {@link FieldProps}.
 */
export const Field = ({ label, children }: FieldProps) => (
    <label className='field'>
        <span>{label}</span>
        {children}
    </label>
);
