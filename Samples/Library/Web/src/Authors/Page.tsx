// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import { Card, EmptyRow, Field, Page, TableCard } from '../Shell';
import type { AuthorDetails } from './AuthorDetails';

const columnCount = 4;

/**
 * The authors the library holds books by. Removing an author the catalog still points at is the
 * rejection this page exists to show.
 */
export const Authors = () => {
    const authors = useCollection<AuthorDetails>('/api/authors');
    const rejection = useRejection();
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');

    const register = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<AuthorDetails>('/api/authors', { firstName, lastName });
            setFirstName('');
            setLastName('');
            await authors.reload();
        });
    };

    const remove = (author: AuthorDetails) =>
        rejection.perform(async () => {
            await apiClient.remove(`/api/authors/${author.authorId}`);
            await authors.reload();
        });

    return (
        <Page
            title='Authors'
            subtitle='The people the library holds books by.'
            problem={rejection.problem ?? authors.problem}>
            <Card title='Register author'>
                <form className='form-row' data-testid='author-form' onSubmit={register}>
                    <Field label='First name'>
                        <input
                            className='input'
                            data-testid='author-first-name'
                            value={firstName}
                            onChange={(event) => setFirstName(event.target.value)}
                        />
                    </Field>
                    <Field label='Last name'>
                        <input
                            className='input'
                            data-testid='author-last-name'
                            value={lastName}
                            onChange={(event) => setLastName(event.target.value)}
                        />
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='author-submit'
                        disabled={rejection.isPerforming}>
                        Register
                    </button>
                </form>
            </Card>

            <TableCard testId='authors-table'>
                <thead>
                    <tr>
                        <th>First name</th>
                        <th>Last name</th>
                        <th className='numeric'>Books</th>
                        <th />
                    </tr>
                </thead>
                <tbody>
                    {authors.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={authors.isLoading}
                            message='No authors registered yet.'
                        />
                    ) : (
                        authors.items.map((author) => (
                            <tr
                                key={author.authorId}
                                data-testid='author-row'
                                data-id={author.authorId}>
                                <td>{author.firstName}</td>
                                <td>{author.lastName}</td>
                                <td className='numeric'>{author.bookCount}</td>
                                <td>
                                    <button
                                        className='button button-danger'
                                        type='button'
                                        data-testid='author-delete'
                                        disabled={rejection.isPerforming}
                                        onClick={() => void remove(author)}>
                                        Remove
                                    </button>
                                </td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
