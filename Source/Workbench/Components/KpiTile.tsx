// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

export interface KpiTileProps {
    readonly label: string;
    readonly value: ReactNode;

    /** A line under the value, such as a trend or a unit. */
    readonly hint?: ReactNode;
}

export const KpiTile = ({ label, value, hint }: KpiTileProps) => (
    <div className="tile">
        <span className="k">{label}</span>
        <span className="v">{value}</span>
        {hint && <span className="s">{hint}</span>}
    </div>
);
