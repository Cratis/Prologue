// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient } from '../Api';
import type { Rejection } from '../Api';
import { Card, Field } from '../Shell';
import type { InventoryDetails } from './InventoryDetails';

const defaultCount = '1';

/**
 * The props for {@link LossForm}.
 */
export interface LossFormProps {
    /**
     * The titles the library actually holds. Losses are picked from the inventory rather than the
     * catalog, since you can only lose copies you hold — which also keeps the "more lost than held"
     * rejection reachable from the rows visible on the page.
     */
    items: InventoryDetails[];

    /** The page's rejection state, so a 404 or 422 lands in the page's one error block. */
    rejection: Rejection;

    /** Re-reads the inventory once copies have been written off, since the counts change. */
    onReported: () => Promise<void>;
}

/**
 * Writes copies of a title off as lost. Reporting against a title the library does not hold is
 * rejected with 404, and writing off more copies than are on the shelf with 422.
 * @param props The {@link LossFormProps}.
 */
export const LossForm = ({ items, rejection, onReported }: LossFormProps) => {
    const [isbn, setIsbn] = useState('');
    const [count, setCount] = useState(defaultCount);

    const report = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<void>(`/api/inventory/${encodeURIComponent(isbn)}/lost`, {
                count: Number(count)
            });

            setCount(defaultCount);
            await onReported();
        });
    };

    return (
        <Card title='Report lost copies'>
            <form className='form-row' data-testid='lost-form' onSubmit={report}>
                <Field label='ISBN'>
                    <select
                        className='select'
                        data-testid='lost-isbn'
                        value={isbn}
                        onChange={(event) => setIsbn(event.target.value)}>
                        <option value=''>Select a title</option>
                        {items.map((item) => (
                            <option key={item.isbn} value={item.isbn}>
                                {item.title} ({item.isbn})
                            </option>
                        ))}
                    </select>
                </Field>
                <Field label='Copies lost'>
                    <input
                        className='input'
                        type='number'
                        min='1'
                        data-testid='lost-count'
                        value={count}
                        onChange={(event) => setCount(event.target.value)}
                    />
                </Field>
                <button
                    className='button'
                    type='submit'
                    data-testid='lost-submit'
                    disabled={rejection.isPerforming}>
                    Report lost
                </button>
            </form>
        </Card>
    );
};
