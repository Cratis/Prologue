// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useEffect } from 'react';
import { OffBanner } from './OffBanner';
import { pages } from './pages';
import { useRouter } from './Router';
import { Sidebar } from './Sidebar';
import { defaultStep, stepForPath } from './steps';
import { TabBar } from './TabBar';

/** The frame every page renders in: sidebar, bottom tab bar, the off-banner of the current step and the page itself. */
export const Shell = () => {
    const { path, navigate } = useRouter();
    const step = stepForPath(path);

    useEffect(() => {
        if (!step) navigate(defaultStep.path);
    }, [step, navigate]);

    const current = step ?? defaultStep;
    const Page = pages[current.id];

    return (
        <>
            <a href="#main" className="skip-link">Skip to content</a>
            <Sidebar />
            <TabBar />
            <main id="main" className="main">
                <div className="page">
                    <OffBanner step={current} />
                    <Page />
                </div>
            </main>
        </>
    );
};
