// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Link } from '../Shell/Router';
import { LoadingState, PageHeader } from '../Components';
import { NewPrologue } from '../Prologues/Defining/NewPrologue';
import { RepositoryConnectionsPanel } from '../Repositories/RepositoryConnectionsPanel';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { SystemsAISlot } from './AISlot';
import { CaptureBoundary } from './CaptureBoundary';
import { EvidencePanel } from './EvidencePanel';
import { GeneratedConfiguration } from './GeneratedConfiguration';
import { RuntimeSourcesPanel } from './RuntimeSources/RuntimeSourcesPanel';
import { SystemsPanel } from './SystemsPanel';

/** The Systems step: the prologue's systems and where their code lives, the evidence it gathers, and what becomes cratis-prologue.json. */
export const Systems = () => {
    const header = useStepHeader('systems');
    const prologue = useCurrentPrologue();
    const { loaded, creating } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue || creating) return <><PageHeader {...header} /><NewPrologue /></>;

    const next = prologue.steps.discover ? '/discover' : '/capture';
    return (
        <>
            <PageHeader {...header} actions={<Link to={next} className="btn primary">Continue</Link>} />
            <div className="grid grid-aside">
                <div className="grid">
                    <SystemsPanel prologue={prologue} />
                    <div className="grid grid-halves">
                        <RepositoryConnectionsPanel />
                        <EvidencePanel />
                    </div>
                    <SystemsAISlot />
                    <RuntimeSourcesPanel prologue={prologue} />
                </div>
                <div className="grid aside-column">
                    <CaptureBoundary prologue={prologue} />
                    <GeneratedConfiguration prologue={prologue} />
                </div>
            </div>
        </>
    );
};
