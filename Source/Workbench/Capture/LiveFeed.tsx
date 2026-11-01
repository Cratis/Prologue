// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';
import { EmptyState, Panel, SegmentedControl } from '../Components';
import { CaptureLike } from './captureTypes';
import { CaptureRow } from './CaptureRow';
import { serviceLabel } from './format';

export interface LiveFeedProps {
    readonly captures: readonly CaptureLike[];
    readonly selectedId: string;
    readonly onSelect: (id: string) => void;
    readonly service: string;
    readonly onServiceChange: (service: string) => void;
    readonly paused: boolean;
    readonly waiting: number;
    readonly children?: ReactNode;
}

const allServices = '__all__';

/** The live feed, newest first, narrowed to one service when the prologue has several. */
export const LiveFeed = ({ captures, selectedId, onSelect, service, onServiceChange, paused, waiting, children }: LiveFeedProps) => {
    const services = [...new Set(captures.map(capture => capture.service))].sort();
    const shown = service === allServices ? captures : captures.filter(capture => capture.service === service);
    const note = paused ? `Paused \u00b7 ${waiting} new ${waiting === 1 ? 'capture is' : 'captures are'} waiting` : 'Newest first \u00b7 click a capture to inspect it';
    return (
        <Panel title="Live feed" aside={note}>
            {children}
            {services.length > 1 && (
                <div className="feed-filter">
                    <SegmentedControl
                        label="Service"
                        value={service}
                        onChange={onServiceChange}
                        options={[{ value: allServices, label: 'All services' }, ...services.map(name => ({ value: name, label: serviceLabel(name) }))]} />
                </div>
            )}
            {shown.length === 0
                ? <EmptyState title="No captures yet" description="Captures appear here as the Extractor sends them. Nothing needs reloading." />
                : <div className="feed">{shown.map(capture => <CaptureRow key={capture.capture} capture={capture} selected={capture.capture === selectedId} onSelect={onSelect} />)}</div>}
        </Panel>
    );
};

export { allServices };
