// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export type ProviderTone = 'gh' | 'ado' | 'gl' | 'fs';

export interface ProviderMarkProps {
    /** The two letters shown in the mark. */
    readonly abbreviation: string;
    readonly tone: ProviderTone;

    /** The provider's name, for the tooltip and assistive technology. */
    readonly label: string;
}

/** A small square mark naming a repository host. */
export const ProviderMark = ({ abbreviation, tone, label }: ProviderMarkProps) => (
    <span className={`prov ${tone}`} title={label} role="img" aria-label={label}>{abbreviation}</span>
);
