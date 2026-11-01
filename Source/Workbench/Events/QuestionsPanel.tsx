// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { FormEvent, useState } from 'react';
import { Button, Panel } from '../Components';
import { OpenQuestion } from './Questions/OpenQuestion';

export interface QuestionsPanelProps {
    readonly questions: readonly OpenQuestion[];
    readonly answers: readonly { readonly key: string; readonly prompt: string; readonly answer: string }[];
    readonly busy: (question: string) => boolean;
    readonly onAnswer: (question: string, answer: string) => void;
    readonly interpreting: boolean;
}

/** The questions the interpreter asked and waits on, with the choices it offers and free text, and the answers already given. */
export const QuestionsPanel = ({ questions, answers, busy, onAnswer, interpreting }: QuestionsPanelProps) => (
    <Panel title="Questions from the interpreter" aside={questions.length > 0 ? `${questions.length} open` : 'none waiting'}>
        <div className="grid">
            {questions.map(question => <QuestionView key={question.id.toString()} question={question} busy={busy(`question-${question.id}`)} onAnswer={onAnswer} />)}
            {questions.length === 0 && (
                <p className="muted small-print">
                    {interpreting ? 'The interpreter is working. Questions it cannot decide on its own appear here.' : 'Nothing waits for an answer. When the interpreter is unsure about something that changes the model, it asks here.'}
                </p>
            )}
            {answers.map(answered => (
                <div key={answered.key} className="q answered">
                    <div>{answered.prompt}</div>
                    <div className="muted small-print">You answered: {answered.answer}</div>
                </div>
            ))}
        </div>
    </Panel>
);

interface QuestionViewProps {
    readonly question: OpenQuestion;
    readonly busy: boolean;
    readonly onAnswer: (question: string, answer: string) => void;
}

const QuestionView = ({ question, busy, onAnswer }: QuestionViewProps) => {
    const [text, setText] = useState('');
    const id = question.id.toString();

    const submit = (event: FormEvent) => {
        event.preventDefault();
        if (text.trim()) onAnswer(id, text.trim());
    };

    return (
        <div className="q">
            <div>{question.prompt}</div>
            {question.context && <div className="muted small-print">{question.context}</div>}
            <div className="opts">
                {question.choices.map(choice => (
                    <button key={choice.label} type="button" disabled={busy} aria-pressed="false" onClick={() => onAnswer(id, choice.label)}>
                        {choice.label}{choice.description && <span className="muted"> · {choice.description}</span>}
                    </button>
                ))}
            </div>
            <form className="free" onSubmit={submit}>
                <input aria-label="Another answer" placeholder="Or answer in your own words" value={text} onChange={event => setText(event.target.value)} disabled={busy} />
                <Button small type="submit" disabled={busy || !text.trim()}>Answer</Button>
            </form>
        </div>
    );
};
