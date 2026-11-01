// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PageHeaderProps } from '../Components';
import { usePrologues } from './shellState';
import { StepId, stepById, steps } from './steps';

/** The props of a step's `PageHeader`: eyebrow ("Step 2 of 7 · Discover · Optional · system"), title and description. */
export const useStepHeader = (id: StepId): Pick<PageHeaderProps, 'eyebrow' | 'title' | 'description'> => {
    const { current } = usePrologues();
    const step = stepById(id);
    const parts = [`Step ${step.number} of ${steps.length}`, step.label, step.optional ? 'Optional' : '', current.system].filter(Boolean);
    return { eyebrow: parts.join(' · '), title: step.title, description: step.description };
};
