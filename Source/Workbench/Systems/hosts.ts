// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ProviderTone } from '../Components';
import { RepositoryHost } from './RepositoryHost';

/** How a repository host is named and marked wherever a system or a connection mentions it. */
export interface HostInfo {
    readonly host: RepositoryHost;
    readonly label: string;
    readonly abbreviation: string;
    readonly tone: ProviderTone;

    /** What the repository field of a system on this host holds. */
    readonly repositoryLabel: string;
    readonly repositoryPlaceholder: string;
}

export const hosts: readonly HostInfo[] = [
    { host: RepositoryHost.gitHub, label: 'GitHub', abbreviation: 'GH', tone: 'gh', repositoryLabel: 'Repository', repositoryPlaceholder: 'cratis/library-platform' },
    { host: RepositoryHost.azureDevOps, label: 'Azure DevOps', abbreviation: 'AD', tone: 'ado', repositoryLabel: 'Project and repository', repositoryPlaceholder: 'contoso/library-ops' },
    { host: RepositoryHost.gitLab, label: 'GitLab', abbreviation: 'GL', tone: 'gl', repositoryLabel: 'Project', repositoryPlaceholder: 'library/notifications' },
    { host: RepositoryHost.localFolder, label: 'Local folder', abbreviation: 'FS', tone: 'fs', repositoryLabel: 'Folder', repositoryPlaceholder: '/srv/legacy' }
];

export const hostInfo = (host: RepositoryHost): HostInfo => hosts.find(info => info.host === host) ?? hosts[0];
