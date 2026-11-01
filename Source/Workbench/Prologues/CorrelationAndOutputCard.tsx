// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Field } from '../Components';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { SetCorrelationWindow } from './Correlation/SetCorrelationWindow';
import { CaptureOutput } from './CaptureOutput';
import { ChooseOutput } from './Output/ChooseOutput';
import { PrologueDefinition } from './PrologueDefinition';

/** The correlation window within which an HTTP command claims what it caused, and where captures are sent. Both apply to the whole prologue. */
export const CorrelationAndOutputCard = ({ prologue }: { prologue: PrologueDefinition }) => {
    const { run } = useCommandRunner();
    const [windowMs, setWindowMs] = useState(prologue.correlation);

    const commitWindow = () => {
        if (windowMs === prologue.correlation) return;
        const command = new SetCorrelationWindow();
        command.prologue = prologue.id;
        command.window = windowMs;
        void run('correlation-window', command, 'Correlation window saved');
    };

    const chooseOutput = (output: CaptureOutput) => {
        const command = new ChooseOutput();
        command.prologue = prologue.id;
        command.kind = output;
        void run('output', command, 'Output saved');
    };

    return (
        <div className="src">
            <header><b>Correlation &amp; output</b></header>
            <Field label={`Correlation window: ${windowMs} ms`} hint="An HTTP command claims database transactions and spans seen within this window. Shared trace ids join regardless of time.">
                <input
                    type="range" min={250} max={5000} step={250} value={windowMs}
                    onChange={event => setWindowMs(Number(event.target.value))}
                    onPointerUp={commitWindow} onKeyUp={commitWindow} onBlur={commitWindow} />
            </Field>
            <Field label="Send captures to">
                <select value={prologue.output} onChange={event => chooseOutput(Number(event.target.value))}>
                    <option value={CaptureOutput.api}>Receiver → MongoDB (live view)</option>
                    <option value={CaptureOutput.json}>JSON files in ./captures</option>
                </select>
            </Field>
        </div>
    );
};
