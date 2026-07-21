// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import type { AuthorDetails } from '../Authors';
import { Card, EmptyRow, Field, Page, TableCard } from '../Shell';
import type { BookDetails } from './BookDetails';
import { TagForm } from './TagForm';

const columnCount = 4;

/**
 * The library's catalog of titles. Registering a book against an author who is not registered is
 * rejected with 422.
 */
export const Catalog = () => {
    const books = useCollection<BookDetails>('/api/catalog/books');
    const authors = useCollection<AuthorDetails>('/api/authors');
    const rejection = useRejection();
    const [isbn, setIsbn] = useState('');
    const [title, setTitle] = useState('');
    const [authorId, setAuthorId] = useState('');

    const register = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<BookDetails>('/api/catalog/books', {
                isbn,
                title,
                authorId: Number(authorId)
            });

            setIsbn('');
            setTitle('');
            await books.reload();
            await authors.reload();
        });
    };

    return (
        <Page
            title='Catalog'
            subtitle='The titles the library has catalogued.'
            problem={rejection.problem ?? books.problem ?? authors.problem}>
            <Card title='Register book'>
                <form className='form-row' data-testid='book-form' onSubmit={register}>
                    <Field label='ISBN'>
                        <input
                            className='input'
                            data-testid='book-isbn'
                            value={isbn}
                            onChange={(event) => setIsbn(event.target.value)}
                        />
                    </Field>
                    <Field label='Title'>
                        <input
                            className='input'
                            data-testid='book-title'
                            value={title}
                            onChange={(event) => setTitle(event.target.value)}
                        />
                    </Field>
                    <Field label='Author'>
                        <select
                            className='select'
                            data-testid='book-author'
                            value={authorId}
                            onChange={(event) => setAuthorId(event.target.value)}>
                            <option value=''>Select an author</option>
                            {authors.items.map((author) => (
                                <option key={author.authorId} value={author.authorId}>
                                    {author.firstName} {author.lastName}
                                </option>
                            ))}
                        </select>
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='book-submit'
                        disabled={rejection.isPerforming}>
                        Register
                    </button>
                </form>
            </Card>

            <TagForm books={books.items} rejection={rejection} onTagged={books.reload} />

            <TableCard testId='books-table'>
                <thead>
                    <tr>
                        <th>ISBN</th>
                        <th>Title</th>
                        <th>Author</th>
                        <th>Tags</th>
                    </tr>
                </thead>
                <tbody>
                    {books.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={books.isLoading}
                            message='No books catalogued yet.'
                        />
                    ) : (
                        books.items.map((book) => (
                            <tr
                                key={book.bookId}
                                data-testid='book-row'
                                data-id={book.bookId}
                                data-isbn={book.isbn}>
                                <td>{book.isbn}</td>
                                <td>{book.title}</td>
                                <td>{book.authorName}</td>
                                <td>
                                    {book.tags.map((tag) => (
                                        <span className='tag' key={tag}>
                                            {tag}
                                        </span>
                                    ))}
                                </td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
