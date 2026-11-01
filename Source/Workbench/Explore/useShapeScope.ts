// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { ExcludeShapeFromInterpretation } from './Scope/Excluding/ExcludeShapeFromInterpretation';
import { IncludeShapeInInterpretation } from './Scope/Excluding/IncludeShapeInInterpretation';
import { SplitShapeByStatus } from './Scope/Splitting/SplitShapeByStatus';
import { UndoShapeSplit } from './Scope/Splitting/UndoShapeSplit';

/**
 * Changes what of a prologue's captures the interpretation looks at. Each control passes through the command runner, so a
 * second press while the command runs is ignored and a refusal is shown as a toast.
 */
export const useShapeScope = (prologue: PrologueDefinition) => {
    const { run, isBusy } = useCommandRunner();

    const exclude = (shape: string) => {
        const command = new ExcludeShapeFromInterpretation();
        command.prologue = prologue.id;
        command.shape = shape;
        return run(`scope-${shape}`, command, 'Left out of the next interpretation');
    };

    const include = (shape: string) => {
        const command = new IncludeShapeInInterpretation();
        command.prologue = prologue.id;
        command.shape = shape;
        return run(`scope-${shape}`, command, 'Part of the next interpretation again');
    };

    const split = (shape: string) => {
        const command = new SplitShapeByStatus();
        command.prologue = prologue.id;
        command.shape = shape;
        return run(`split-${shape}`, command, 'Split into one shape per response status. The interpreter will treat each as its own candidate.');
    };

    const undoSplit = (shape: string) => {
        const command = new UndoShapeSplit();
        command.prologue = prologue.id;
        command.shape = shape;
        return run(`split-${shape}`, command, 'Interpreted as one shape again');
    };

    return { exclude, include, split, undoSplit, isBusy };
};
