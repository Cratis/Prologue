// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Field, Panel } from '../Components';
import { useSliceDraft } from './useSliceDraft';

export interface AddSliceFormProps {
    /** The command shapes captured or found in the code, to pick from. */
    readonly knownShapes: readonly string[];

    /** The shapes slices already replace. */
    readonly taken: readonly string[];
    readonly busy: boolean;
    readonly onAdd: (name: string, shapes: string[]) => Promise<boolean>;
}

/** Adds a slice to rebuild: a name and the legacy command shapes it replaces, picked from what was captured or typed. */
export const AddSliceForm = ({ knownShapes, taken, busy, onAdd }: AddSliceFormProps) => {
    const draft = useSliceDraft();
    const available = knownShapes.filter(shape => !taken.includes(shape));

    const submit = async () => {
        const typed = draft.typed.trim();
        if (await onAdd(draft.name, typed === '' || draft.shapes.includes(typed) ? [...draft.shapes] : [...draft.shapes, typed])) draft.clear();
    };

    return (
        <Panel title="Add a slice" aside="The new slice replaces one or more legacy commands">
            <form className="stack" onSubmit={event => { event.preventDefault(); void submit(); }}>
                <div className="add-slice">
                    <Field label="Slice name">
                        <input value={draft.name} onChange={event => draft.setName(event.target.value)} placeholder="LendBook" />
                    </Field>
                    <Field label="Legacy command" hint="Pick a captured command, or type one as METHOD /path">
                        <input
                            value={draft.typed}
                            onChange={event => draft.setTyped(event.target.value)}
                            onKeyDown={event => { if (event.key === 'Enter') { event.preventDefault(); draft.addTyped(); } }}
                            placeholder="POST /api/loans" />
                    </Field>
                    <Button type="submit" variant="primary" disabled={busy || draft.name.trim() === '' || (draft.shapes.length === 0 && draft.typed.trim() === '')}>Add slice</Button>
                </div>
                {(available.length > 0 || draft.shapes.length > 0) && (
                    <div className="shape-picks" role="group" aria-label="Legacy commands">
                        {[...new Set([...draft.shapes, ...available])].map(shape => (
                            <button key={shape} type="button" className="btn small" aria-pressed={draft.shapes.includes(shape)} onClick={() => draft.toggle(shape)}>{shape}</button>
                        ))}
                    </div>
                )}
            </form>
        </Panel>
    );
};
