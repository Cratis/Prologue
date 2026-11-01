// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The Workbench's steps: the one place that says what the seven steps are, where they live and how they read.
 *
 * To add or change a step, edit this file and register its page component in `Shell/pages.ts`. The sidebar, the bottom
 * tab bar, the router, the page header and the off-banner all read from here.
 */

export type StepId = 'systems' | 'discover' | 'capture' | 'explore' | 'events' | 'model' | 'rebuild';

export type StepGroupName = 'Setup' | 'Evidence' | 'Interpret' | 'Modernize';

export interface OffBannerCopy {
    /** Says that the step is off. */
    readonly headline: string;

    /** Says what is lost while the step is off. */
    readonly consequence: string;
}

export interface Step {
    readonly id: StepId;
    readonly path: string;
    readonly number: number;
    readonly label: string;
    readonly group: StepGroupName;

    /** An optional step can be turned off for a prologue. */
    readonly optional: boolean;

    /** The page heading. */
    readonly title: string;

    /** The sentence under the page heading. */
    readonly description: string;

    /** Only optional steps carry copy for the banner shown while they are off. */
    readonly offBanner?: OffBannerCopy;
}

export interface StepGroup {
    readonly name: StepGroupName;

    /** A small marker next to the group name, such as "Experimental". */
    readonly note?: string;
    readonly steps: readonly Step[];
}

export const steps: readonly Step[] = [
    {
        id: 'systems', path: '/systems', number: 1, label: 'Systems', group: 'Setup', optional: false,
        title: 'Define the systems and evidence',
        description: 'List the systems that make up the prologue, where each one’s code lives, and which evidence Prologue should gather. Discovering code and capturing runtime are both optional; use either or both. Everything here becomes cratis-prologue.json.'
    },
    {
        id: 'discover', path: '/discover', number: 2, label: 'Discover', group: 'Evidence', optional: true,
        title: 'Map what the code does',
        description: 'AI agents read each system’s source and map its capabilities, the decisions it makes and what talks to what. Point it at a repository path or a folder on disk. With the local pool, code never leaves your network.',
        offBanner: { headline: 'Discover is off for this prologue.', consequence: 'Events will come from runtime capture only.' }
    },
    {
        id: 'capture', path: '/capture', number: 3, label: 'Capture', group: 'Evidence', optional: true,
        title: 'Watch captures arrive',
        description: 'Each capture is one HTTP command plus the database transactions and spans correlated with it. Label what you are doing so the interpreter knows the business intent.',
        offBanner: { headline: 'Capture is off for this prologue.', consequence: 'Events will come from the code alone, so none of them is backed by observed behavior.' }
    },
    {
        id: 'explore', path: '/explore', number: 4, label: 'Explore', group: 'Interpret', optional: false,
        title: 'Explore what the system does',
        description: 'Captures grouped into command shapes. The matrix shows which tables each command writes; partial cells mean the write happens only in some runs. Exclude noise before interpreting.'
    },
    {
        id: 'events', path: '/events', number: 5, label: 'Events', group: 'Interpret', optional: false,
        title: 'Review event suggestions',
        description: 'The interpreter proposes past-tense facts from the evidence. Mechanical names come from tables and routes; refined names come from the language model. Nothing becomes part of the model until you accept it.'
    },
    {
        id: 'model', path: '/model', number: 6, label: 'Model', group: 'Interpret', optional: false,
        title: 'Provisional event model',
        description: 'Accepted events laid out as slices along the story of the system. Dashed outlines are still open. Select a slice to see its evidence and the Screenplay it will produce.'
    },
    {
        id: 'rebuild', path: '/rebuild', number: 7, label: 'Rebuild', group: 'Modernize', optional: true,
        title: 'Rebuild piece by piece',
        description: 'Each slice moves from observed to cut over on its own. Before a route switches, the new slice replays the captured commands and its decisions are compared with what the legacy system decided.',
        offBanner: { headline: 'Rebuild tracking is off.', consequence: 'Turn it on when you start building slices from the model.' }
    }
];

const groupNotes: Partial<Record<StepGroupName, string>> = { Setup: 'Experimental' };

/** The steps in their sidebar groups, in order. */
export const stepGroups: readonly StepGroup[] = (['Setup', 'Evidence', 'Interpret', 'Modernize'] as const).map(name => ({
    name,
    note: groupNotes[name],
    steps: steps.filter(step => step.group === name)
}));

/** The step shown when the path names none. */
export const defaultStep: Step = steps[0];

export const stepById = (id: StepId): Step => steps.find(step => step.id === id) ?? defaultStep;

export const stepForPath = (path: string): Step | undefined => steps.find(step => step.path === path);
