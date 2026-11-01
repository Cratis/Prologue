// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '../Components';
import { useOptionalSteps } from './shellState';
import { Step } from './steps';

/** Explains what is lost while an optional step is off and offers to turn it on. Renders nothing for any other step. */
export const OffBanner = ({ step }: { step: Step }) => {
    const { isOn, setOn } = useOptionalSteps();

    if (!step.optional || isOn(step.id) || !step.offBanner) return null;

    return (
        <div className="offbanner" role="region" aria-label={`${step.label} is off`}>
            <b>{step.offBanner.headline}</b>
            <span className="muted">{step.offBanner.consequence}</span>
            <Button small onClick={() => setOn(step.id, true)}>Turn on</Button>
        </div>
    );
};
