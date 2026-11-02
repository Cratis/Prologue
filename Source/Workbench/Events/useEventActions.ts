// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { useCommandRunner } from '../Shell/useCommandRunner';
import { ChooseConsistencyBoundary } from './Boundaries/ChooseConsistencyBoundary';
import { RunInterpretation } from './Interpreting/RunInterpretation';
import { AnswerInterpreterQuestion } from './Questions/AnswerInterpreterQuestion';
import { AcceptSuggestion } from './Reviewing/AcceptSuggestion';
import { RejectSuggestion } from './Reviewing/RejectSuggestion';
import { RenameSuggestion } from './Reviewing/RenameSuggestion';
import { RestoreSuggestion } from './Reviewing/RestoreSuggestion';

/**
 * The things a person does on the Events page. Each control passes through the command runner, so a second press while its command runs
 * is ignored and a refusal (such as accepting what was rejected) is shown as a toast.
 */
export const useEventActions = (prologue: PrologueDefinition) => {
    const { run, isBusy } = useCommandRunner();

    const interpret = () => {
        const command = new RunInterpretation();
        command.prologue = prologue.id;
        return run('interpret', command, 'Interpreting in the background');
    };

    const accept = (key: string) => {
        const command = new AcceptSuggestion();
        command.prologue = prologue.id;
        command.key = key;
        return run(`event-${key}`, command);
    };

    const reject = (key: string) => {
        const command = new RejectSuggestion();
        command.prologue = prologue.id;
        command.key = key;
        return run(`event-${key}`, command);
    };

    const restore = (key: string) => {
        const command = new RestoreSuggestion();
        command.prologue = prologue.id;
        command.key = key;
        return run(`event-${key}`, command);
    };

    const rename = (key: string, name: string) => {
        const command = new RenameSuggestion();
        command.prologue = prologue.id;
        command.key = key;
        command.name = name;
        return run(`event-${key}`, command);
    };

    /**
     * Answers a question. A question the Workbench raised itself (standalone) has no interpretation waiting for it, so the answer is fed
     * back by interpreting again; the interpreter's own questions carry on the interpretation that waits.
     */
    const answer = async (question: string, text: string, standalone = false) => {
        const command = new AnswerInterpreterQuestion();
        command.prologue = prologue.id;
        command.question = Guid.parse(question);
        command.answer = text;
        const answered = await run(`question-${question}`, command, standalone ? 'Answer recorded. Interpreting again with it.' : 'Answer recorded. The interpreter carries on with it.');
        if (answered && standalone) await interpret();
        return answered;
    };

    const choose = (boundary: string, option: string) => {
        const command = new ChooseConsistencyBoundary();
        command.prologue = prologue.id;
        command.boundary = boundary;
        command.option = option;
        return run(`boundary-${boundary}`, command, 'Boundary decision recorded. It appears in the model as a rule.');
    };

    return { interpret, accept, reject, restore, rename, answer, choose, isBusy };
};

export type EventActions = ReturnType<typeof useEventActions>;
