// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Panel, Toggle } from '../Components';
import { useOptionalSteps } from '../Shell/shellState';
import { StepId } from '../Shell/steps';

const evidence: readonly { readonly id: StepId; readonly label: string; readonly description: string }[] = [
    { id: 'discover', label: 'Discover · analyze source code', description: 'AI agents map capabilities, decisions and dependencies' },
    { id: 'capture', label: 'Capture · watch the running system', description: 'HTTP commands, database changes and telemetry' },
    { id: 'rebuild', label: 'Rebuild · track the new system', description: 'Parity against captured decisions as slices go live' }
];

/** Which evidence the prologue gathers. Discover or Capture stays on; the server refuses turning off the last of them. */
export const EvidencePanel = () => {
    const { isOn, setOn } = useOptionalSteps();

    return (
        <Panel title="Evidence for this prologue" aside="Each step is optional">
            <div className="scen">
                {evidence.map(step => (
                    <div key={step.id} className="scen-item">
                        <span className="grow">
                            <b>{step.label}</b>
                            <span className="muted block small-text">{step.description}</span>
                        </span>
                        <Toggle label={step.label} hideLabel checked={isOn(step.id)} onChange={checked => setOn(step.id, checked)} />
                    </div>
                ))}
            </div>
            <p className="muted small-text note">
                At least one of Discover and Capture has to be on. With both, findings in the code are matched to what the running system actually did.
            </p>
        </Panel>
    );
};
