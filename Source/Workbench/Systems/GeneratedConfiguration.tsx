// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CopyButton, Panel } from '../Components';
import { ObserveGeneratedConfiguration } from '../Prologues/Configuration/GeneratedConfiguration';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';

/** The `cratis-prologue.json` for the prologue, kept current as the definition changes. Secrets appear by name only, and captures are posted back to this Receiver. */
export const GeneratedConfiguration = ({ prologue }: { prologue: PrologueDefinition }) => {
    const [result] = ObserveGeneratedConfiguration.use({ id: prologue.id, receiver: window.location.origin });
    const json = result.data?.json ?? '';

    return (
        <Panel title="cratis-prologue.json" aside={<CopyButton text={json} label="Copy configuration" />}>
            <pre className="code" aria-label="Generated configuration">{json}</pre>
        </Panel>
    );
};
