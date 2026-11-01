// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useRouter } from './Router';
import { usePrologues } from './shellState';

const newPrologue = '__new__';

/** Switches between prologues; a native select sits invisibly over the styled box so it stays keyboard and screen-reader friendly. */
export const PrologueSwitcher = () => {
    const { prologues, current, selectPrologue, hasPrologue, setCreating } = usePrologues();
    const { navigate } = useRouter();

    const choose = (value: string) => {
        if (value === newPrologue) {
            setCreating(true);
            navigate('/systems');
            return;
        }
        selectPrologue(value);
    };

    return (
        <div className="switcher">
            <span className="who">
                <span className="mark" aria-hidden="true">{current.name.charAt(0).toUpperCase()}</span>
                <span className="titles">
                    <span className="name">{current.name}</span>
                    <small>{current.system || 'Define one to begin'}</small>
                </span>
            </span>
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="m7 9 5-5 5 5M7 15l5 5 5-5" />
            </svg>
            <select aria-label="Switch prologue" value={current.id} onChange={event => choose(event.target.value)}>
                {!hasPrologue && <option value="">{current.name}</option>}
                {prologues.map(prologue => <option key={prologue.id} value={prologue.id}>{prologue.name}</option>)}
                <option value={newPrologue}>New prologue…</option>
            </select>
        </div>
    );
};
