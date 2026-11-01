// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { usePrologues } from './shellState';

/** Switches between prologues; a native select sits invisibly over the styled box so it stays keyboard and screen-reader friendly. */
export const PrologueSwitcher = () => {
    const { prologues, current, selectPrologue } = usePrologues();

    return (
        <div className="switcher">
            <span className="who">
                <span className="mark" aria-hidden="true">{current.name.charAt(0)}</span>
                <span className="titles">
                    <span className="name">{current.name}</span>
                    <small>{current.system}</small>
                </span>
            </span>
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="m7 9 5-5 5 5M7 15l5 5 5-5" />
            </svg>
            <select aria-label="Switch prologue" value={current.id} onChange={event => selectPrologue(event.target.value)}>
                {prologues.map(prologue => <option key={prologue.id} value={prologue.id}>{prologue.name}</option>)}
            </select>
        </div>
    );
};
