// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { scenarioStatuses } from '../scenarioStatus';

describe('when working out the status of each scenario', () => {
    const statuses = scenarioStatuses(['Lend a copy', 'Report copies lost'], [{ scenario: 'Lend a copy', captures: 902 }, { scenario: 'Removed one', captures: 3 }]);

    it('marks a scenario with labeled captures as captured', () => expect(statuses[0]).toEqual({ name: 'Lend a copy', captures: 902, captured: true }));
    it('marks a scenario without labeled captures as not run yet', () => expect(statuses[1]).toEqual({ name: 'Report copies lost', captures: 0, captured: false }));
    it('lists only the scenarios in the script', () => expect(statuses).toHaveLength(2));
});
