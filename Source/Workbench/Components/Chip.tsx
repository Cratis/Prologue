// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';

/** The meaning a chip carries: http, db and otel name capture sources; the rest are statuses. */
export type ChipVariant = 'neutral' | 'http' | 'db' | 'otel' | 'ok' | 'warn' | 'drift' | 'bad';

export interface ChipProps {
    readonly variant?: ChipVariant;
    readonly children: ReactNode;
}

export const Chip = ({ variant = 'neutral', children }: ChipProps) => (
    <span className={variant === 'neutral' ? 'chip' : `chip ${variant}`}>{children}</span>
);
