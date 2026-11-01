// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Panel } from '../../Components';
import { useAISetup } from '../useAISetup';
import { AgentsSection } from './AgentsSection';
import { PoolsSection } from './PoolsSection';
import { ProvidersSection } from './ProvidersSection';

/** The AI panel: Cratis.AI providers, pools and agents, set up the way Cratis Direct sets them up. */
export const AISettings = () => {
    const setup = useAISetup();

    return (
        <Panel title="AI" aside="Cratis.AI providers, pools and agents">
            <div className="ai-grid">
                <ProvidersSection setup={setup} />
                <PoolsSection setup={setup} />
                <AgentsSection setup={setup} />
            </div>
        </Panel>
    );
};
