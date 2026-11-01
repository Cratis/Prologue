// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { KpiTile } from '../Components';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { CaptureData } from './useCaptureData';
import { capturesPerMinute, minuteSeries, sparklinePoints } from './sparkline';
import { traceLinkedShare } from './format';

const sparkWidth = 160;
const sparkHeight = 30;

const Sparkline = ({ series }: { series: readonly number[] }) => {
    const points = sparklinePoints(series, sparkWidth, sparkHeight);
    return (
        <svg className="spark" width="100%" height={sparkHeight} viewBox={`0 0 ${sparkWidth} ${sparkHeight}`} preserveAspectRatio="none" aria-hidden="true">
            {points && <polyline className="area" points={`0,${sparkHeight} ${points} ${sparkWidth},${sparkHeight}`} />}
            {points && <polyline className="line" points={points} />}
        </svg>
    );
};

const watchedTableCount = (prologue: PrologueDefinition) =>
    new Set(prologue.systems.flatMap(system => system.databases.flatMap(database => database.tables))).size;

/** The five numbers above the feed. Every one is read from an aggregate kept up to date as captures arrive. */
export const CaptureKpis = ({ prologue, data }: { prologue: PrologueDefinition; data: CaptureData }) => {
    const watched = watchedTableCount(prologue);
    const share = traceLinkedShare(data.traceLinked, data.withCommand);
    return (
        <div className="tiles">
            <KpiTile
                label="Captures"
                value={data.total.toLocaleString()}
                hint={<>{`${capturesPerMinute(data.buckets)} per minute`}<Sparkline series={minuteSeries(data.buckets)} /></>} />
            <KpiTile label="Command shapes seen" value={data.shapes.length} hint="Unseen routes need a scenario" />
            <KpiTile
                label="Tables touched"
                value={<>{data.tables.length}{watched > 0 && <span className="muted of"> / {watched}</span>}</>}
                hint={watched > 0 ? 'of the watched schema' : 'by captured transactions'} />
            <KpiTile label="Trace-linked" value={share === undefined ? '\u2014' : `${share}%`} hint="The rest correlate by time only" />
            <KpiTile label="Uncorrelated transactions" value={<span className={data.uncorrelated > 0 ? 'warn-text' : undefined}>{data.uncorrelated}</span>} hint="Database writes with no command" />
        </div>
    );
};
