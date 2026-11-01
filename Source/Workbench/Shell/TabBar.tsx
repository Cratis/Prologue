// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Link, useRouter } from './Router';
import { steps } from './steps';

/** The seven steps as a bottom bar, shown instead of the sidebar below 820 px. */
export const TabBar = () => {
    const { path } = useRouter();

    return (
        <nav className="tabbar" aria-label="Steps">
            {steps.map(step => (
                <Link key={step.id} to={step.path} aria-current={path === step.path ? 'page' : undefined}>
                    <span className="n">{step.number}</span>
                    {step.label}
                </Link>
            ))}
        </nav>
    );
};
