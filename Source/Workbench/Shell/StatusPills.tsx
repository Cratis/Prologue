// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { postureLabels } from '../AI/networkPosture';
import { useCaptureStatus } from './shellState';

const postureTitles = {
    localOnly: 'Every AI pool member runs inside your network',
    localFirst: 'Each AI pool starts inside your network and pauses before going outside',
    leavesNetwork: 'Some AI work may be sent to a provider outside your network'
};

export const StatusPills = () => {
    const { capturing, elapsed, posture } = useCaptureStatus();

    return (
        <div className="status">
            <span className={capturing ? 'status-pill live' : 'status-pill paused'}>
                <span className="dot" aria-hidden="true" />
                {capturing ? `Capturing · ${elapsed}` : 'Not capturing'}
            </span>
            {posture !== 'none' && (
                <span className={posture === 'leavesNetwork' ? 'status-pill paused' : 'status-pill local'} title={postureTitles[posture]}>{postureLabels[posture]}</span>
            )}
        </div>
    );
};
