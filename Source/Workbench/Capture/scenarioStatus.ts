// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface ScenarioStatus {
    readonly name: string;
    readonly captures: number;
    readonly captured: boolean;
}

/**
 * The status of each scenario in a script: captured when at least one capture carries its label, otherwise not run yet.
 * @param scenarios The scenarios in the script, in order.
 * @param counts The captures per scenario label.
 */
export const scenarioStatuses = (
    scenarios: readonly string[],
    counts: readonly { readonly scenario: string; readonly captures: number }[]
): ScenarioStatus[] =>
    scenarios.map(name => {
        const captures = counts.find(count => count.scenario === name)?.captures ?? 0;
        return { name, captures, captured: captures > 0 };
    });
