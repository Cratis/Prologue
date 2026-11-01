// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { PrologueSystem } from '../Systems/PrologueSystem';
import { isActive } from './discoverState';
import { AnalyzeFolder } from './Analyzing/AnalyzeFolder';
import { AnalyzeSystemCode } from './Analyzing/AnalyzeSystemCode';
import { AnalysisStatus } from './Map/AnalysisStatus';
import { ShareMap } from './Sharing/ShareMap';
import { StopSharingMap } from './Sharing/StopSharingMap';

/**
 * What can be done on the Discover step: analyze a system, a folder or everything again, and share the map. Each command
 * runs through the command runner, so a control can be disabled while it runs and a refusal is shown to the user.
 * @param prologue The prologue.
 * @param systems The prologue's systems, to tell a system from an analyzed folder.
 * @returns The actions and whether a keyed one is running.
 */
export const useDiscoverActions = (prologue: Guid, systems: readonly PrologueSystem[]) => {
    const { run, isBusy } = useCommandRunner();

    const analyzeSystem = (system: Guid, name: string) => {
        const command = new AnalyzeSystemCode();
        command.prologue = prologue;
        command.id = system;
        return run(`analyze-${system}`, command, `Analyzing ${name}`);
    };

    const analyzeFolder = (folder: string) => {
        const command = new AnalyzeFolder();
        command.prologue = prologue;
        command.folder = folder;
        return run(`analyze-folder-${folder}`, command, `${folder} queued for analysis`);
    };

    const analyzeAgain = async (statuses: readonly AnalysisStatus[]) => {
        for (const status of statuses.filter(candidate => !isActive(candidate.state))) {
            const isSystem = systems.some(system => String(system.id) === String(status.subject));
            await (isSystem ? analyzeSystem(status.subject, status.name) : analyzeFolder(status.repository));
        }
    };

    const share = () => run('share-map', Object.assign(new ShareMap(), { prologue }), 'The map is shared');
    const stopSharing = () => run('stop-sharing-map', Object.assign(new StopSharingMap(), { prologue }), 'The map is no longer shared');

    return { analyzeSystem, analyzeFolder, analyzeAgain, share, stopSharing, isBusy };
};
