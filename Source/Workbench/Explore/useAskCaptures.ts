// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useRef, useState } from 'react';
import { PrologueDefinition } from '../Prologues/PrologueDefinition';
import { AnswerStatus } from './Asking/AnswerStatus';
import { AskAboutCaptures } from './Asking/CaptureAnswer';

/** An answer on the page: from the Interpreter agent, or worked out from the matrix when the agent could not be asked. */
export interface Answer {
    readonly question: string;
    readonly text: string;
    readonly citations: readonly string[];

    /** Why the agent did not answer; empty when it did. */
    readonly reason: string;

    /** Whether the text was worked out from the matrix instead of answered by the agent. */
    readonly fromMatrix: boolean;
}

/**
 * Asks questions about the captures. Only the latest question counts: an answer that arrives after another question was
 * asked is dropped. When the agent cannot be asked and the question is one of the suggested ones, the answer the matrix gives
 * on its own is shown, labeled as such.
 */
export const useAskCaptures = (
    prologue: PrologueDefinition,
    suggestions: readonly { readonly question: string; readonly answer: string; readonly citations: readonly string[] }[]
) => {
    const [question, setQuestion] = useState('');
    const [answer, setAnswer] = useState<Answer | undefined>();
    const [asking, setAsking] = useState(false);
    const latest = useRef(0);

    const worked = useCallback((text: string): Answer | undefined => {
        const match = suggestions.find(suggestion => suggestion.question === text);
        return match && { question: text, text: match.answer, citations: match.citations, reason: '', fromMatrix: true };
    }, [suggestions]);

    const ask = useCallback(async (text: string) => {
        const trimmed = text.trim();
        if (trimmed === '') return;
        const turn = ++latest.current;
        setQuestion(trimmed);
        setAsking(true);
        const result = await new AskAboutCaptures().perform({ id: prologue.id, question: trimmed });
        if (turn !== latest.current) return;
        setAsking(false);
        const reply = result.data;
        if (result.isSuccess && reply?.status === AnswerStatus.answered) {
            setAnswer({ question: trimmed, text: reply.text, citations: reply.citations, reason: '', fromMatrix: false });
            return;
        }

        const reason = result.isSuccess ? reply?.reason ?? '' : 'The question could not be sent';
        const fallback = worked(trimmed);
        setAnswer(fallback ? { ...fallback, reason } : { question: trimmed, text: '', citations: [], reason, fromMatrix: false });
    }, [prologue.id, worked]);

    const answerFromMatrix = useCallback((text: string) => {
        latest.current++;
        setQuestion(text);
        setAsking(false);
        setAnswer(worked(text));
    }, [worked]);

    return { question, setQuestion, answer, asking, ask, answerFromMatrix };
};
