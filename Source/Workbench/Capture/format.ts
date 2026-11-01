// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChangeOperation } from '../ChangeOperation';

/** The share of captures with a command that were joined to their spans by trace, as a whole percentage, or undefined when there are none. */
export const traceLinkedShare = (traceLinked: number, withCommand: number): number | undefined =>
    withCommand === 0 ? undefined : Math.round((traceLinked / withCommand) * 100);

const operations: Record<ChangeOperation, string> = {
    [ChangeOperation.insert]: 'INSERT',
    [ChangeOperation.update]: 'UPDATE',
    [ChangeOperation.delete]: 'DELETE'
};

/** The SQL word for what was done to a row. */
export const operationName = (operation: ChangeOperation): string => operations[operation];

/** The first letter of the operation, for the small op badge. */
export const operationLetter = (operation: ChangeOperation): string => operations[operation][0];

/** The time of day of a capture, such as 12:03:41. */
export const clock = (at: Date): string => at.toLocaleTimeString([], { hour12: false });

/** The service a capture belongs to, or a label for the default unnamed service. */
export const serviceLabel = (service: string): string => service || 'Default service';
