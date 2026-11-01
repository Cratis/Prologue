// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChipVariant } from '../Components';
import { AnalysisState } from './AnalysisState';
import { DecisionKind } from './DecisionKind';
import { AnalysisStatus } from './Map/AnalysisStatus';
import { ApplicationMap } from './Map/ApplicationMap';
import { CodeDecisionPoint } from './Map/CodeDecisionPoint';
import { DecisionRuntime } from './Runtime/DecisionRuntime';

const plural = (count: number, one: string, many: string) => `${count.toLocaleString('en-US')} ${count === 1 ? one : many}`;

/**
 * Says how often a decision point happened at runtime, in the words the table shows.
 * @param point The decision point.
 * @returns "72 refusals (409)", "902 accepted", "14 uncorrelated writes" or "never seen".
 */
export const describeHappened = (point: Pick<CodeDecisionPoint, 'kind' | 'happened' | 'status' | 'runtime'>): string => {
    if (point.runtime === DecisionRuntime.codeOnly) return 'never seen';
    if (point.runtime === DecisionRuntime.explainsWrites) return plural(point.happened, 'uncorrelated write', 'uncorrelated writes');
    if (point.kind === DecisionKind.refusal) {
        const status = point.status > 0 ? ` (${point.status})` : '';
        return `${plural(point.happened, 'refusal', 'refusals')}${status}`;
    }
    return `${point.happened.toLocaleString('en-US')} accepted`;
};

export interface RuntimeChip {
    readonly variant: ChipVariant;
    readonly label: string;
}

/**
 * The chip for how a decision point stands against the runtime evidence.
 * @param runtime The decision point's runtime match.
 * @returns The chip: Matches, Explains uncorrelated writes, or Code only.
 */
export const runtimeChip = (runtime: DecisionRuntime): RuntimeChip => {
    switch (runtime) {
        case DecisionRuntime.matches: return { variant: 'ok', label: 'Matches' };
        case DecisionRuntime.explainsWrites: return { variant: 'ok', label: 'Explains uncorrelated writes' };
        default: return { variant: 'warn', label: 'Code only' };
    }
};

/** Whether an analysis is waiting or running. */
export const isActive = (state: AnalysisState): boolean => state === AnalysisState.queued || state === AnalysisState.running;

export interface DiscoverKpis {
    readonly filesRead: number;
    readonly filesTotal: number;
    readonly activeAnalyses: number;
    readonly capabilities: number;
    readonly systems: number;
    readonly decisionPoints: number;
    readonly matched: number;
    readonly codeOnly: number;
}

/**
 * Works out the numbers on the Discover tiles. While an analysis runs, files read and total come from its progress;
 * otherwise from the last analysis.
 * @param map The application map, when it has arrived.
 * @param statuses The analysis statuses.
 * @returns The numbers.
 */
export const discoverKpis = (map: ApplicationMap | undefined, statuses: readonly AnalysisStatus[]): DiscoverKpis => ({
    filesRead: statuses.length > 0 ? statuses.reduce((sum, status) => sum + status.filesRead, 0) : map?.filesRead ?? 0,
    filesTotal: statuses.reduce((sum, status) => sum + status.totalFiles, 0),
    activeAnalyses: statuses.filter(status => isActive(status.state)).length,
    capabilities: map?.capabilities ?? 0,
    systems: map?.systems.length ?? 0,
    decisionPoints: map?.decisionPoints ?? 0,
    matched: map?.matchedToRuntime ?? 0,
    codeOnly: map?.codeOnly ?? 0
});

/** The line under the tile of files read. */
export const analysisSummary = (kpis: DiscoverKpis, statuses: readonly AnalysisStatus[]): string => {
    if (kpis.activeAnalyses > 0) return `${plural(kpis.activeAnalyses, 'analysis', 'analyses')} running`;
    if (statuses.some(status => status.state === AnalysisState.failed)) return 'An analysis could not finish';
    if (statuses.some(status => status.state === AnalysisState.complete)) return 'Analysis complete';
    return 'Nothing analyzed yet';
};

/**
 * The line that says where an analysis stands.
 * @param status The status.
 * @returns For example "scanning 3 of 10", "analyzed a41c9e2" or "waiting".
 */
export const standing = (status: Pick<AnalysisStatus, 'state' | 'filesRead' | 'totalFiles' | 'commit'>): string => {
    switch (status.state) {
        case AnalysisState.queued: return 'waiting for its turn';
        case AnalysisState.running: return `scanning ${status.filesRead} of ${status.totalFiles}`;
        case AnalysisState.complete: return `analyzed${status.commit ? ` ${String(status.commit).slice(0, 7)}` : ''}`;
        case AnalysisState.failed: return 'could not finish';
        default: return 'not analyzed yet';
    }
};

export type FileMarker = 'done' | 'current' | 'waiting';

/**
 * Whether a listed file has been read, is being read or waits.
 * @param index The file's position in the list.
 * @param filesRead How many files have been read.
 * @param state Where the analysis stands.
 * @returns The marker.
 */
export const fileMarker = (index: number, filesRead: number, state: AnalysisState): FileMarker => {
    if (state === AnalysisState.complete || index < filesRead) return 'done';
    return index === filesRead && state === AnalysisState.running ? 'current' : 'waiting';
};

const sharePrefix = '/share/';

/**
 * Reads the token out of a read-only link's path.
 * @param path The location's path.
 * @returns The token, or undefined when the path is not a shared map.
 */
export const shareTokenFromPath = (path: string): string | undefined => {
    if (!path.startsWith(sharePrefix)) return undefined;
    const token = decodeURIComponent(path.slice(sharePrefix.length).split('/')[0]);
    return token.length > 0 ? token : undefined;
};

/** The path of the read-only link for a token. */
export const sharePath = (token: string): string => `${sharePrefix}${encodeURIComponent(token)}`;
