// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** Whether an agent answers in a conversation or runs as a background job inside the Receiver. */
export type AgentMode = 'chat' | 'job';

/** An agent of Prologue as the panel describes it. */
export interface PrologueAgentInfo {
    /** The purpose Cratis.AI knows the agent by; it matches `PrologueAgents` on the backend. */
    readonly purpose: string;
    readonly name: string;
    readonly mark: string;
    readonly mode: AgentMode;
    readonly summary: string;
}

/**
 * Prologue's four agents. Cratis.AI configures an agent by its purpose; the mode is a fact about what the Workbench
 * asks the agent to do, so it is shown but not changed here.
 */
export const prologueAgents: readonly PrologueAgentInfo[] = [
    { purpose: 'prologue-dramaturg', name: 'Dramaturg', mark: 'Dr', mode: 'job', summary: 'Reads each system’s source inside the Receiver and maps its modules and capabilities.' },
    { purpose: 'prologue-prompter', name: 'Prompter', mark: 'Pr', mode: 'job', summary: 'Finds where the code decides (rules, refusals, state changes) with file and line.' },
    { purpose: 'prologue-usher', name: 'Usher', mark: 'Us', mode: 'chat', summary: 'Follows calls between systems and confirms them against captured spans.' },
    { purpose: 'prologue-interpreter', name: 'Interpreter', mark: 'In', mode: 'chat', summary: 'Names events, refines suggestions and asks you the questions in Events.' }
];
