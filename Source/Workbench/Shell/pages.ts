// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentType } from 'react';
import { Capture } from '../Capture/Capture';
import { Discover } from '../Discover/Discover';
import { Events } from '../Events/Events';
import { Explore } from '../Explore/Explore';
import { Model } from '../Model/Model';
import { Rebuild } from '../Rebuild/Rebuild';
import { Systems } from '../Systems/Systems';
import { StepId } from './steps';

/**
 * The page component of every step. A `Record` keyed by `StepId`, so a step added in `steps.ts` does not compile until
 * it has a page here.
 */
export const pages: Record<StepId, ComponentType> = {
    systems: Systems,
    discover: Discover,
    capture: Capture,
    explore: Explore,
    events: Events,
    model: Model,
    rebuild: Rebuild
};
