// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** What a command result says when it did not succeed: the refusal, the exception or the authorization failure. */
export interface FailedResult {
    readonly validationResults: readonly { readonly message: string }[];
    readonly exceptionMessages: readonly string[];
    readonly authorizationFailureReason?: string;
}

const longestReason = 120;

/** The outcome message a command result carries when it did not succeed. */
export const failureMessage = (result: FailedResult): string =>
    result.validationResults[0]?.message || result.exceptionMessages[0] || result.authorizationFailureReason || 'The command did not succeed';

/**
 * The message for a command that never got an answer - the request was rejected before a result existed, such as the
 * server being unreachable. Only a short single-line reason is shown; anything longer (a stack, a payload) is left out.
 */
export const rejectionMessage = (reason: unknown): string => {
    const text = reason instanceof Error ? reason.message.trim() : '';
    const showable = text.length > 0 && text.length <= longestReason && !text.includes('\n');
    return showable ? `The command could not be sent: ${text}` : 'The command could not be sent';
};

/** What running a command came to: whether it succeeded and the message to show (the failure, or the success message). */
export interface CommandOutcome {
    readonly succeeded: boolean;
    readonly message?: string;
}

/** Executes a command and turns every way it can end - success, refusal, or a rejection before any answer - into an outcome. */
export const executeCommand = async (
    command: { readonly execute: () => Promise<FailedResult & { readonly isSuccess: boolean }> },
    success?: string
): Promise<CommandOutcome> => {
    try {
        const result = await command.execute();
        return result.isSuccess ? { succeeded: true, message: success } : { succeeded: false, message: failureMessage(result) };
    } catch (reason) {
        return { succeeded: false, message: rejectionMessage(reason) };
    }
};
