// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';
import { Button } from '../../Components';

export interface RuntimeSourceCardProps {
    readonly title: string;
    readonly children: ReactNode;

    /** Whether the draft differs from what is saved. */
    readonly changed: boolean;
    readonly busy: boolean;
    readonly onSave: () => void;
}

/** One runtime source of a system: its fields and a Save that is only available when something changed. */
export const RuntimeSourceCard = ({ title, children, changed, busy, onSave }: RuntimeSourceCardProps) => (
    <div className="src">
        <header><b>{title}</b></header>
        {children}
        <div className="row">
            <Button small variant="primary" disabled={!changed || busy} onClick={onSave}>Save</Button>
        </div>
    </div>
);
