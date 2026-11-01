// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisMark } from './CratisMark';
import { PrologueSwitcher } from './PrologueSwitcher';
import { Link, useRouter } from './Router';
import { useOptionalSteps, useStepBadges } from './shellState';
import { StatusPills } from './StatusPills';
import { stepGroups } from './steps';

export const Sidebar = () => {
    const { path } = useRouter();
    const { isOn } = useOptionalSteps();
    const badges = useStepBadges();

    return (
        <nav className="side" aria-label="Primary">
            <Link to="/systems" className="wordmark"><CratisMark />Cratis Prologue</Link>
            <PrologueSwitcher />
            {stepGroups.map(group => (
                <div key={group.name} className="navgroup" role="group" aria-label={group.name}>
                    <div className="navhead" aria-hidden="true">{group.name}{group.note && <span>{group.note}</span>}</div>
                    {group.steps.map(step => (
                        <Link key={step.id} to={step.path} className="nav" aria-current={path === step.path ? 'page' : undefined}>
                            <span className="n" aria-hidden="true">{step.number}</span>
                            {step.label}
                            {badges[step.id] ? <span className="badge" aria-label={`${badges[step.id]} open questions`}>{badges[step.id]}</span> : null}
                            {step.optional && <span className={isOn(step.id) ? 'opt' : 'opt off'}>{isOn(step.id) ? 'Optional' : 'Off'}</span>}
                        </Link>
                    ))}
                </div>
            ))}
            <div className="spacer" />
            <StatusPills />
        </nav>
    );
};
