// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { EmptyState, LoadingState, PageHeader } from '../Components';
import { ObservePrologue } from '../Prologues/PrologueDefinition';
import { ApplicationMapPanel } from './ApplicationMapPanel';
import { DecisionPointsPanel } from './DecisionPointsPanel';
import { DependencyDiagram } from './DependencyDiagram';
import { DiscoverTiles } from './DiscoverTiles';
import { discoverKpis } from './discoverState';
import { ObserveSharedMapByToken } from './Sharing/SharedMap';
import { useDiscoverMap } from './useDiscoverData';

/** The read-only map a share link opens. It has no shell and no controls that change anything. */
export const SharedMapPage = ({ token }: { token: string }) => {
    const [shares] = ObserveSharedMapByToken.use({ token });
    const share = shares.data[0];

    if (!shares.hasData) return <main className="share-page"><LoadingState label="Opening the shared map" /></main>;
    if (!share) return <main className="share-page"><EmptyState title="This link does not open a map" description="Sharing may have been stopped, or the link is mistyped. Ask whoever sent it for a new one." /></main>;

    return <SharedMap prologue={share.id} />;
};

const SharedMap = ({ prologue }: { prologue: Guid }) => {
    const [definition] = ObservePrologue.use({ id: prologue });
    const { map, decisionPoints, dependencies, loaded } = useDiscoverMap(prologue);
    const kpis = discoverKpis(map, []);

    return (
        <main className="share-page">
            <PageHeader
                eyebrow="Shared read-only · Discover"
                title={definition.hasData && definition.data.name ? `${definition.data.name}: what the code does` : 'What the code does'}
                description="This is a read-only view of a map shared from Prologue. It shows what the analysis found in the code, matched to what the running system did." />
            <DiscoverTiles kpis={kpis} summary="Shared from Prologue" />
            {!loaded
                ? <LoadingState label="Loading the map" />
                : (
                    <div className="grid">
                        <ApplicationMapPanel map={map} statuses={[]} readOnly />
                        <DecisionPointsPanel decisionPoints={decisionPoints} readOnly />
                        <DependencyDiagram dependencies={dependencies} />
                    </div>
                )}
        </main>
    );
};
