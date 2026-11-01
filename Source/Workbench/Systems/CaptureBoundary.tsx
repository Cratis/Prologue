// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Panel } from '../Components';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { neverRecorded, recordedItems } from './captureBoundary';

/** What Prologue records for this prologue and what it never records, whatever is configured. */
export const CaptureBoundary = ({ prologue }: { prologue: PrologueDefinition }) => {
    const recorded = recordedItems(prologue);

    return (
        <Panel title="Capture boundary">
            <div className="boundary">
                <div>
                    <b className="good">Recorded</b>
                    <ul>{recorded.length > 0 ? recorded.map(item => <li key={item}>{item}</li>) : <li className="muted">Nothing yet</li>}</ul>
                </div>
                <div>
                    <b className="bad">Never recorded</b>
                    <ul>{neverRecorded.map(item => <li key={item}>{item}</li>)}</ul>
                </div>
            </div>
            <p className="muted small-text note">Query strings and allowlisted attribute values can still be sensitive. Protect capture storage.</p>
        </Panel>
    );
};
