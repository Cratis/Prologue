// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import type { BookDetails } from '../Catalog';
import { Card, EmptyRow, Field, Page, TableCard } from '../Shell';
import type { InventoryDetails } from './InventoryDetails';
import { LossForm } from './LossForm';

const columnCount = 5;
const defaultCount = '1';

/**
 * How many copies of each title the library holds. Registering copies of an uncatalogued ISBN is
 * rejected with 422.
 */
export const Inventory = () => {
    const inventory = useCollection<InventoryDetails>('/api/inventory');
    const books = useCollection<BookDetails>('/api/catalog/books');
    const rejection = useRejection();
    const [isbn, setIsbn] = useState('');
    const [count, setCount] = useState(defaultCount);

    const register = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<InventoryDetails>('/api/inventory', {
                isbn,
                count: Number(count)
            });

            setCount(defaultCount);
            await inventory.reload();
        });
    };

    return (
        <Page
            title='Inventory'
            subtitle='How many copies of each title the library holds.'
            problem={rejection.problem ?? inventory.problem ?? books.problem}>
            <Card title='Register copies'>
                <form className='form-row' data-testid='inventory-form' onSubmit={register}>
                    <Field label='ISBN'>
                        <select
                            className='select'
                            data-testid='inventory-isbn'
                            value={isbn}
                            onChange={(event) => setIsbn(event.target.value)}>
                            <option value=''>Select a title</option>
                            {books.items.map((book) => (
                                <option key={book.bookId} value={book.isbn}>
                                    {book.title} ({book.isbn})
                                </option>
                            ))}
                        </select>
                    </Field>
                    <Field label='Copies'>
                        <input
                            className='input'
                            type='number'
                            min='1'
                            data-testid='inventory-count'
                            value={count}
                            onChange={(event) => setCount(event.target.value)}
                        />
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='inventory-submit'
                        disabled={rejection.isPerforming}>
                        Register
                    </button>
                </form>
            </Card>

            <LossForm
                items={inventory.items}
                rejection={rejection}
                onReported={inventory.reload}
            />

            <TableCard testId='inventory-table'>
                <thead>
                    <tr>
                        <th>ISBN</th>
                        <th>Title</th>
                        <th>Author</th>
                        <th className='numeric'>Owned</th>
                        <th className='numeric'>Available</th>
                    </tr>
                </thead>
                <tbody>
                    {inventory.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={inventory.isLoading}
                            message='No copies held yet.'
                        />
                    ) : (
                        inventory.items.map((item) => (
                            <tr
                                key={item.isbn}
                                data-testid='inventory-row'
                                data-isbn={item.isbn}>
                                <td>{item.isbn}</td>
                                <td>{item.title}</td>
                                <td>{item.authorName}</td>
                                <td className='numeric'>{item.totalCount}</td>
                                <td className='numeric'>{item.availableCount}</td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
