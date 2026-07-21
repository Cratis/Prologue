// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient } from '../Api';
import type { Rejection } from '../Api';
import { Card, Field } from '../Shell';
import type { BookDetails } from './BookDetails';

/**
 * The props for {@link TagForm}.
 */
export interface TagFormProps {
    /** The catalogued titles to pick from. */
    books: BookDetails[];

    /** The page's rejection state, so a 404 or 409 lands in the page's one error block. */
    rejection: Rejection;

    /** Re-reads the catalog once a tag has been attached, since the table shows tags. */
    onTagged: () => Promise<void>;
}

/**
 * Attaches a free-text tag to a catalogued title. Tagging an unknown book is rejected with 404, and
 * attaching the same tag twice with 409.
 * @param props The {@link TagFormProps}.
 */
export const TagForm = ({ books, rejection, onTagged }: TagFormProps) => {
    const [isbn, setIsbn] = useState('');
    const [tag, setTag] = useState('');

    const attach = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<void>(
                `/api/catalog/books/${encodeURIComponent(isbn)}/tags`,
                { tag }
            );

            setTag('');
            await onTagged();
        });
    };

    return (
        <Card title='Tag a book'>
            <form className='form-row' data-testid='tag-form' onSubmit={attach}>
                <Field label='ISBN'>
                    <select
                        className='select'
                        data-testid='tag-isbn'
                        value={isbn}
                        onChange={(event) => setIsbn(event.target.value)}>
                        <option value=''>Select a title</option>
                        {books.map((book) => (
                            <option key={book.bookId} value={book.isbn}>
                                {book.title} ({book.isbn})
                            </option>
                        ))}
                    </select>
                </Field>
                <Field label='Tag'>
                    <input
                        className='input'
                        data-testid='tag-value'
                        value={tag}
                        onChange={(event) => setTag(event.target.value)}
                    />
                </Field>
                <button
                    className='button'
                    type='submit'
                    data-testid='tag-submit'
                    disabled={rejection.isPerforming}>
                    Attach
                </button>
            </form>
        </Card>
    );
};
