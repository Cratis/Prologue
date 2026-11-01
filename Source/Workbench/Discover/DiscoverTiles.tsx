// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { KpiTile } from '../Components';
import { DiscoverKpis } from './discoverState';

export interface DiscoverTilesProps {
    readonly kpis: DiscoverKpis;
    readonly summary: string;
}

const muted = (text: string) => <span className="muted tile-total">{text}</span>;

/** The five Discover tiles: files read, capabilities mapped, decision points, matched to runtime and found only in code. */
export const DiscoverTiles = ({ kpis, summary }: DiscoverTilesProps) => (
    <div className="tiles">
        <KpiTile label="Files read" value={<>{kpis.filesRead.toLocaleString('en-US')}{kpis.filesTotal > 0 && muted(` / ${kpis.filesTotal.toLocaleString('en-US')}`)}</>} hint={summary} />
        <KpiTile label="Capabilities mapped" value={kpis.capabilities} hint={`across ${kpis.systems} ${kpis.systems === 1 ? 'system' : 'systems'}`} />
        <KpiTile label="Decision points" value={kpis.decisionPoints} hint="rules the code enforces" />
        <KpiTile label="Matched to runtime" value={<>{kpis.matched}{muted(` / ${kpis.capabilities}`)}</>} hint="capabilities also seen in captures" />
        <KpiTile label="Found only in code" value={<span className={kpis.codeOnly > 0 ? 'warn-text' : undefined}>{kpis.codeOnly}</span>} hint="never exercised during capture" />
    </div>
);
