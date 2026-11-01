// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Chip } from '../../Components';
import { RepositoryHost } from '../../Systems/RepositoryHost';
import { CloneStatus } from './CloneStatus';
import { SystemClone } from './SystemClone';

export interface CloneStatusChipProps {
    /** Where the system's code lives; a system with no location has nothing to clone. */
    readonly located: boolean;
    readonly host?: RepositoryHost;
    readonly clone?: SystemClone;
}

/** The clone status of a system: the commit and file count when cloned, the redacted reason when it failed. */
export const CloneStatusChip = ({ located, host, clone }: CloneStatusChipProps) => {
    if (!located) return <Chip>No code location</Chip>;

    if (clone?.status === CloneStatus.failed) {
        return <span title={clone.reason}><Chip variant="bad">Clone failed</Chip></span>;
    }

    if (clone?.status === CloneStatus.cloned) {
        const files = `${clone.files} file${clone.files === 1 ? '' : 's'}`;
        return <span title={`Cloned commit ${clone.commit}`}><Chip variant="ok">{`${String(clone.commit).slice(0, 7)} · ${files}`}</Chip></span>;
    }

    return <Chip variant="warn">{host === RepositoryHost.localFolder ? 'Not read yet' : 'Not cloned'}</Chip>;
};
