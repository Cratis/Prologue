// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { FormEvent } from 'react';
import { Button, Panel } from '../Components';
import { Answer } from './useAskCaptures';

export interface AskPanelProps {
    readonly question: string;
    readonly onQuestion: (question: string) => void;
    readonly onAsk: (question: string) => void;
    readonly onAnswerFromMatrix: (question: string) => void;
    readonly asking: boolean;
    readonly answer?: Answer;
    readonly suggestions: readonly { readonly question: string }[];

    /** Whether the Interpreter agent can be asked; false once the AI setup answered that it has no provider. */
    readonly canAsk: boolean;

    /** How many captures each shape has, for the count on a citation. */
    readonly captures: ReadonlyMap<string, number>;
    readonly onSelectShape: (shape: string) => void;
}

/** Questions about the captures, with suggestions, and an answer that cites the command shapes it rests on. */
export const AskPanel = ({ question, onQuestion, onAsk, onAnswerFromMatrix, asking, answer, suggestions, canAsk, captures, onSelectShape }: AskPanelProps) => {
    const submit = (event: FormEvent) => {
        event.preventDefault();
        if (canAsk) onAsk(question);
    };

    return (
        <Panel title="Ask about the captures" aside="Answers cite the command shapes they come from">
            <form className="ask" onSubmit={submit}>
                <input
                    value={question}
                    onChange={event => onQuestion(event.target.value)}
                    placeholder="Ask, for example: what happens when a book is lent?"
                    aria-label="Question about captures"
                    disabled={!canAsk} />
                <Button variant="primary" type="submit" disabled={!canAsk || asking || question.trim() === ''}>{asking ? 'Asking' : 'Ask'}</Button>
            </form>
            {!canAsk && (
                <p className="muted note" role="status">
                    No AI provider is configured for the Interpreter agent, so free questions cannot be answered. The suggestions below
                    are answered from the matrix. Set up a provider in the AI panel to ask your own.
                </p>
            )}
            <div className="suggest">
                {suggestions.map(suggestion => (
                    <Button key={suggestion.question} small disabled={asking} onClick={() => (canAsk ? onAsk(suggestion.question) : onAnswerFromMatrix(suggestion.question))}>
                        {suggestion.question}
                    </Button>
                ))}
            </div>
            {answer && <AnswerView answer={answer} captures={captures} onSelectShape={onSelectShape} />}
        </Panel>
    );
};

const sentence = (reason: string): string => reason.replace(/[.\s]+$/, '');

const AnswerView = ({ answer, captures, onSelectShape }: { answer: Answer; captures: ReadonlyMap<string, number>; onSelectShape: (shape: string) => void }) => {
    if (answer.text === '') {
        return <div className="answer muted" role="status">The Interpreter could not answer{answer.reason ? `: ${sentence(answer.reason)}` : ''}.</div>;
    }

    const cited = answer.citations.filter(shape => captures.has(shape));
    return (
        <div className="answer" role="status">
            <div>{answer.text}</div>
            {answer.fromMatrix && (
                <div className="muted note">Worked out from the matrix{answer.reason ? `. The Interpreter could not answer: ${sentence(answer.reason)}` : ', without an AI provider'}.</div>
            )}
            {cited.length > 0 && (
                <div className="cite">
                    {cited.map(shape => (
                        <button type="button" key={shape} className="chip http" onClick={() => onSelectShape(shape)}>{shape} · {captures.get(shape)}</button>
                    ))}
                </div>
            )}
        </div>
    );
};
