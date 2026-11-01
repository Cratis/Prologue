// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DecisionOutcome } from '../Decisions/DecisionOutcome';
import { ReasonSource } from '../Decisions/ReasonSource';
import { CaptureLike, ReasonLike } from './captureTypes';
import { operationName } from './format';

export interface ExplanationStep {
    readonly text: string;
    /** The evidence the step rests on. */
    readonly evidence: string;
}

const writes = (capture: CaptureLike): string[] =>
    capture.transactions.flatMap(transaction => transaction.tables.map(change => `${operationName(change.operation)} ${change.table}`));

/**
 * Explains a decision in steps, each citing its evidence: the request, what was written (or that nothing was), and the likely rule.
 * A rule Discover found cites its file and line; otherwise the step says what the capture alone suggests.
 * @param capture The capture the decision was observed in.
 * @param reason Why the system decided what it did.
 * @param windowMilliseconds The correlation window, which bounds what "nothing was written" means.
 */
export const explainDecision = (capture: CaptureLike, reason: ReasonLike | undefined, windowMilliseconds: number): ExplanationStep[] => {
    const refused = capture.outcome === DecisionOutcome.refused;
    const written = [...new Set(writes(capture))];
    const request: ExplanationStep = {
        text: `The system received ${capture.http.method} ${capture.http.path} and ${refused ? 'refused' : 'accepted'} it.`,
        evidence: `${capture.http.method} ${capture.http.path} \u2192 ${capture.http.status}`
    };
    const result: ExplanationStep = written.length > 0
        ? { text: `It wrote to ${written.length === 1 ? 'one table' : `${written.length} tables`}.`, evidence: written.join(', ') }
        : { text: 'Nothing was written.', evidence: `No database transaction within the ${windowMilliseconds} ms correlation window` };

    const steps = [request, result];
    if (reason?.source === ReasonSource.discovered) {
        steps.push({ text: `The rule behind it: ${reason.text}.`, evidence: `${reason.file}:${reason.line}` });
    } else if (refused && written.length === 0) {
        steps.push({ text: 'Likely rule: something refused the command before any write. That is evidence for a validation rule.', evidence: 'Observed pattern in this capture' });
    } else if (reason) {
        steps.push({ text: `Observed: ${reason.text}.`, evidence: 'Observed pattern in this capture' });
    }

    return steps;
};
