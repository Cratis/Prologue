// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ReactNode } from 'react';
import type { ProblemDetails } from '../Api';

/**
 * The props for {@link Page}.
 */
export interface PageProps {
    /** The page heading. */
    title: string;

    /** The line under the heading explaining what the page is for. */
    subtitle: string;

    /** The problem to show in the error block, when there is one. */
    problem?: ProblemDetails;

    /** The cards making up the page. */
    children: ReactNode;
}

/**
 * The page frame — heading, subtitle, and the one error block a rejection surfaces in.
 * @param props The {@link PageProps}.
 */
export const Page = ({ title, subtitle, problem, children }: PageProps) => (
    <main className='page'>
        <h1 className='page-title' data-testid='page-title'>
            {title}
        </h1>
        <p className='page-subtitle'>{subtitle}</p>

        {problem && (
            <div className='message message-error' data-testid='error'>
                <strong>{problem.title}</strong> {problem.detail}
            </div>
        )}

        {children}
    </main>
);
