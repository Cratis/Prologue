// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, EmptyState, LoadingState, PageHeader } from '../Components';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { ApplicationMapPanel } from './ApplicationMapPanel';
import { DecisionPointsPanel } from './DecisionPointsPanel';
import { DependencyDiagram } from './DependencyDiagram';
import { DiscoverAICard } from './DiscoverAICard';
import { DiscoverTiles } from './DiscoverTiles';
import { ScanningPanel } from './ScanningPanel';
import { ShareMapControl } from './ShareMapControl';
import { analysisSummary, discoverKpis, isActive } from './discoverState';
import { useAnalysisStatuses, useDiscoverMap } from './useDiscoverData';
import { useDiscoverActions } from './useDiscoverActions';

/** The Discover step: AI agents read each system's source and map its capabilities, decisions and dependencies, matched to what was captured. */
export const Discover = () => {
    const header = useStepHeader('discover');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="Discover works on the systems of a prologue." /></>;

    return <DiscoverBody prologue={prologue} header={header} />;
};

const DiscoverBody = ({ prologue, header }: { prologue: PrologueDefinition; header: ReturnType<typeof useStepHeader> }) => {
    const { map, decisionPoints, dependencies, loaded } = useDiscoverMap(prologue.id);
    const statuses = useAnalysisStatuses(prologue.id);
    const actions = useDiscoverActions(prologue.id, prologue.systems);
    const kpis = discoverKpis(map, statuses);
    const running = statuses.some(status => isActive(status.state));

    return (
        <>
            <PageHeader
                {...header}
                actions={
                    <>
                        <ShareMapControl prologue={prologue.id} actions={actions} />
                        <Button variant="primary" disabled={running || statuses.length === 0} onClick={() => void actions.analyzeAgain(statuses)}>Analyze again</Button>
                    </>
                } />
            <DiscoverTiles kpis={kpis} summary={analysisSummary(kpis, statuses)} />
            {!loaded
                ? <LoadingState label="Loading the map" />
                : (
                    <div className="grid grid-aside">
                        <div className="grid">
                            <ApplicationMapPanel map={map} statuses={statuses} />
                            <DecisionPointsPanel decisionPoints={decisionPoints} />
                        </div>
                        <div className="grid aside-column">
                            <ScanningPanel
                                statuses={statuses}
                                busy={running}
                                onAnalyze={status => void (prologue.systems.some(system => String(system.id) === String(status.subject)) ? actions.analyzeSystem(status.subject, status.name) : actions.analyzeFolder(status.repository))}
                                onAnalyzeFolder={folder => void actions.analyzeFolder(folder)} />
                            <DependencyDiagram dependencies={dependencies} />
                            <DiscoverAICard />
                        </div>
                    </div>
                )}
        </>
    );
};
