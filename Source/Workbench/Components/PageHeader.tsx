// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

export interface PageHeaderProps {
    /** The small uppercase line above the title. */
    readonly eyebrow: string;
    readonly title: string;
    readonly description?: string;

    /** Buttons and links shown to the right of the title. */
    readonly actions?: ReactNode;
}

export const PageHeader = ({ eyebrow, title, description, actions }: PageHeaderProps) => (
    <header className="page-head">
        <div>
            <div className="eyebrow">{eyebrow}</div>
            <h1>{title}</h1>
            {description && <p>{description}</p>}
        </div>
        {actions && <div className="row">{actions}</div>}
    </header>
);
