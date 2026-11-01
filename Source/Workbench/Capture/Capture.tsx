// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Button, EmptyState, LoadingState, PageHeader } from '../Components';
import { Link } from '../Shell/Router';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { BusinessEvents } from './BusinessEvents';
import { CaptureKpis } from './CaptureKpis';
import { CoverageByRoute } from './CoverageByRoute';
import { allServices, LiveFeed } from './LiveFeed';
import { ScenarioMarker } from './ScenarioMarker';
import { ScenarioScriptPanel } from './ScenarioScriptPanel';
import { scenarioStatuses } from './scenarioStatus';
import { SelectedCapture } from './SelectedCapture';
import { useCaptureData } from './useCaptureData';
import { useDiscoveredShapes } from './useDiscoveredShapes';
import { useFeedPause } from './useFeedPause';
import { useScenarioMarking } from './useScenarioMarking';

const defaultWindow = 2000;

/** The Capture step: captures arriving live, the scenario being run, and the decisions they show with their reasons. */
export const Capture = () => {
    const header = useStepHeader('capture');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="Captures belong to a prologue and its systems." action={<Link to="/systems" className="btn primary">Go to Systems</Link>} /></>;
    return <CapturePage />;
};

const CapturePage = () => {
    const header = useStepHeader('capture');
    const prologue = useCurrentPrologue()!;
    const live = useCaptureData(prologue);
    const { view, paused, pause, resume } = useFeedPause(live);
    const marking = useScenarioMarking(prologue, live.activeScenario);
    const known = useDiscoveredShapes();
    const [selectedId, setSelectedId] = useState('');
    const [service, setService] = useState(allServices);

    const window = prologue.correlation || defaultWindow;
    const selected = view.captures.find(capture => String(capture.capture) === selectedId) ?? view.captures[0];
    const reason = view.decisions.find(decision => String(decision.capture) === String(selected?.capture))?.reason;

    return (
        <>
            <PageHeader
                {...header}
                actions={<>
                    <Button onClick={paused ? resume : pause}>{paused ? 'Resume' : 'Pause'}</Button>
                    <Link to="/explore" className="btn primary">Explore captures</Link>
                </>} />
            <CaptureKpis prologue={prologue} data={live} />
            <div className="grid grid-capture">
                <LiveFeed
                    captures={view.captures.map(capture => ({ ...capture, capture: String(capture.capture) }))}
                    selectedId={selected ? String(selected.capture) : ''}
                    onSelect={setSelectedId}
                    service={service}
                    onServiceChange={setService}
                    paused={paused}
                    waiting={Math.max(0, live.total - view.total)}>
                    <ScenarioMarker
                        scenarios={live.scenarios}
                        active={live.activeScenario}
                        typed={marking.typed}
                        onTyped={marking.setTyped}
                        onStart={marking.markStart}
                        onEnd={marking.markEnd}
                        busy={marking.isBusy('mark-scenario')} />
                </LiveFeed>
                <div className="grid aside-column">
                    <SelectedCapture capture={selected && { ...selected, capture: String(selected.capture) }} reason={reason} windowMilliseconds={window} />
                    <BusinessEvents decisions={view.decisions} />
                    <CoverageByRoute data={view} known={known} />
                    <ScenarioScriptPanel
                        statuses={scenarioStatuses(live.scenarios, live.scenarioCounts)}
                        active={live.activeScenario}
                        adding={marking.adding}
                        onAdding={marking.setAdding}
                        onAdd={marking.add}
                        onRemove={marking.remove}
                        isBusy={marking.isBusy} />
                </div>
            </div>
        </>
    );
};
