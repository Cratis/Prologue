// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

export interface PanelProps {
    readonly title?: string;

    /** Secondary text on the right of the title. */
    readonly aside?: ReactNode;
    readonly children: ReactNode;
}

export const Panel = ({ title, aside, children }: PanelProps) => (
    <section className="panel">
        {title && <h3>{title}{aside && <small>{aside}</small>}</h3>}
        {children}
    </section>
);
