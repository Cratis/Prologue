// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ReactNode } from 'react';

/**
 * The props for {@link Card}.
 */
export interface CardProps {
    /** The card heading; omitted for cards that only hold a table. */
    title?: string;

    /** The card content. */
    children: ReactNode;
}

/**
 * A panel on a page.
 * @param props The {@link CardProps}.
 */
export const Card = ({ title, children }: CardProps) => (
    <section className='card'>
        {title && <h2 className='card-title'>{title}</h2>}
        {children}
    </section>
);
