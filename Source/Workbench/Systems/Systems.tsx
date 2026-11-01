// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { EmptyState, PageHeader, Panel } from '../Components';
import { useStepHeader } from '../Shell/useStepHeader';

/** The Systems step. A placeholder until its slices are built; see `Shell/steps.ts` for how a step is described. */
export const Systems = () => (
    <>
        <PageHeader {...useStepHeader('systems')} />
        <Panel>
            <EmptyState title="Coming soon" description="This step is built in a later part of the Workbench." />
        </Panel>
    </>
);
