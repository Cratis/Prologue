// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { EmptyState, LoadingState, PageHeader } from '../Components';
import { Link } from '../Shell/Router';
import { useCurrentPrologue, usePrologues } from '../Shell/shellState';
import { useStepHeader } from '../Shell/useStepHeader';
import { AskPanel } from './AskPanel';
import { JourneysPanel } from './JourneysPanel';
import { MatrixTable } from './MatrixTable';
import { ShapeDetail } from './ShapeDetail';
import { UncorrelatedDetail } from './UncorrelatedDetail';
import { orderedShapes } from './matrix';
import { uncorrelatedKey } from './uncorrelatedKey';
import { useAskCaptures } from './useAskCaptures';
import { useExploreData } from './useExploreData';
import { useInterpreterReady } from './useInterpreterReady';
import { useShapeScope } from './useShapeScope';

/** The Explore step: what the captures say the system does, as a command by table matrix, journeys, and answers to questions. */
export const Explore = () => {
    const header = useStepHeader('explore');
    const prologue = useCurrentPrologue();
    const { loaded } = usePrologues();

    if (!loaded) return <><PageHeader {...header} /><LoadingState label="Loading prologues" /></>;
    if (!prologue) return <><PageHeader {...header} /><EmptyState title="Define a prologue first" description="Captures belong to a prologue and its systems." action={<Link to="/systems" className="btn primary">Go to Systems</Link>} /></>;
    return <ExplorePage />;
};

const ExplorePage = () => {
    const header = useStepHeader('explore');
    const prologue = useCurrentPrologue()!;
    const data = useExploreData(prologue);
    const scope = useShapeScope(prologue);
    const interpreter = useInterpreterReady();
    const asking = useAskCaptures(prologue, data.suggestions);
    const [selection, setSelection] = useState('');

    const actions = <Link to="/events" className="btn primary">Interpret into events</Link>;
    if (!data.ready) return <><PageHeader {...header} actions={actions} /><LoadingState label="Loading captures" /></>;
    if (data.shapes.length === 0 && data.uncorrelated.length === 0) {
        return (
            <>
                <PageHeader {...header} actions={actions} />
                <EmptyState title="Nothing captured yet" description="Explore groups captures into command shapes. Run the system while the Extractor watches it." action={<Link to="/capture" className="btn">Go to Capture</Link>} />
            </>
        );
    }

    const ordered = orderedShapes(data.shapes);
    const selectedShape = ordered.find(shape => shape.shape === selection) ?? (selection === uncorrelatedKey ? undefined : ordered[0]);
    const showUncorrelated = selection === uncorrelatedKey || (ordered.length === 0 && data.uncorrelated.length > 0);

    return (
        <>
            <PageHeader {...header} actions={actions} />
            <div className="grid explore">
                <AskPanel
                    question={asking.question}
                    onQuestion={asking.setQuestion}
                    onAsk={asking.ask}
                    onAnswerFromMatrix={asking.answerFromMatrix}
                    asking={asking.asking}
                    answer={asking.answer}
                    suggestions={data.suggestions}
                    canAsk={!interpreter.loaded || interpreter.configured}
                    captures={new Map(data.shapes.map(shape => [shape.shape, shape.captures]))}
                    onSelectShape={setSelection} />
                <JourneysPanel sequences={data.sequences} />
                <div className="grid grid-aside">
                    <MatrixTable
                        shapes={data.shapes}
                        uncorrelated={data.uncorrelated}
                        excluded={data.excluded}
                        split={data.split}
                        selected={showUncorrelated ? uncorrelatedKey : selectedShape?.shape ?? ''}
                        onSelect={setSelection}
                        onExclude={scope.exclude}
                        onInclude={scope.include}
                        isBusy={scope.isBusy} />
                    {showUncorrelated || !selectedShape
                        ? <UncorrelatedDetail tables={data.uncorrelated} />
                        : (
                            <ShapeDetail
                                shape={selectedShape}
                                excluded={data.excluded.has(selectedShape.shape)}
                                split={data.split.has(selectedShape.shape)}
                                isBusy={scope.isBusy}
                                onExclude={scope.exclude}
                                onInclude={scope.include}
                                onSplit={scope.split}
                                onUndoSplit={scope.undoSplit} />
                        )}
                </div>
            </div>
        </>
    );
};
