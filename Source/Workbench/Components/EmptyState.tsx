// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

export interface EmptyStateProps {
    readonly title: string;
    readonly description?: string;
    readonly action?: ReactNode;
}

export const EmptyState = ({ title, description, action }: EmptyStateProps) => (
    <div className="state">
        <h3>{title}</h3>
        {description && <p>{description}</p>}
        {action}
    </div>
);
