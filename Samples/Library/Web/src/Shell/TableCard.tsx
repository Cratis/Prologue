// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ReactNode } from 'react';
import { Card } from './Card';

/**
 * The props for {@link TableCard}.
 */
export interface TableCardProps {
    /** The test id the shared Playwright suite finds the table by. */
    testId: string;

    /** The table head and body. */
    children: ReactNode;
}

/**
 * The card, scroll container, and table every listing page shares. Each page still writes its own head
 * and rows, so the markup contract stays readable where it is used.
 * @param props The {@link TableCardProps}.
 */
export const TableCard = ({ testId, children }: TableCardProps) => (
    <Card>
        <div className='table-scroll'>
            <table className='table' data-testid={testId}>
                {children}
            </table>
        </div>
    </Card>
);
