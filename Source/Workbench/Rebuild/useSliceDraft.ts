// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';

/** The slice being drafted in the add form: its name and the legacy command shapes it replaces, picked or typed. */
export const useSliceDraft = () => {
    const [name, setName] = useState('');
    const [shapes, setShapes] = useState<readonly string[]>([]);
    const [typed, setTyped] = useState('');

    const toggle = (shape: string) => setShapes(current => current.includes(shape) ? current.filter(held => held !== shape) : [...current, shape]);
    const addTyped = () => {
        const shape = typed.trim();
        if (shape !== '' && !shapes.includes(shape)) setShapes(current => [...current, shape]);
        setTyped('');
    };
    const clear = () => {
        setName('');
        setShapes([]);
        setTyped('');
    };

    return { name, setName, shapes, toggle, typed, setTyped, addTyped, clear };
};
