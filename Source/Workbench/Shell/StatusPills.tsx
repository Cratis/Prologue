// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCaptureStatus } from './shellState';

export const StatusPills = () => {
    const { capturing, elapsed, localOnly } = useCaptureStatus();

    return (
        <div className="status">
            <span className={capturing ? 'status-pill live' : 'status-pill paused'}>
                <span className="dot" aria-hidden="true" />
                {capturing ? `Capturing · ${elapsed}` : 'Not capturing'}
            </span>
            {localOnly && <span className="status-pill local" title="Captures and interpretation stay inside your network">Local only</span>}
        </div>
    );
};
