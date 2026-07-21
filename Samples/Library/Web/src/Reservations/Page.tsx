// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import type { BookDetails } from '../Catalog';
import type { MemberDetails } from '../Members';
import { Card, EmptyRow, Field, formatDateTime, Page, TableCard } from '../Shell';
import type { ReservationDetails } from './ReservationDetails';

const columnCount = 4;

/**
 * Members' claims on copies of titles. Reserving a title with nothing on the shelf is rejected with
 * 422 — the rule this slice exists for.
 */
export const Reservations = () => {
    const reservations = useCollection<ReservationDetails>('/api/reservations');
    const books = useCollection<BookDetails>('/api/catalog/books');
    const members = useCollection<MemberDetails>('/api/members');
    const rejection = useRejection();
    const [isbn, setIsbn] = useState('');
    const [memberId, setMemberId] = useState('');

    const reserve = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<ReservationDetails>('/api/reservations', {
                isbn,
                memberId: Number(memberId)
            });

            await reservations.reload();
        });
    };

    return (
        <Page
            title='Reservations'
            subtitle='Who has claimed a copy of what.'
            problem={
                rejection.problem ?? reservations.problem ?? books.problem ?? members.problem
            }>
            <Card title='Reserve a copy'>
                <form className='form-row' data-testid='reservation-form' onSubmit={reserve}>
                    <Field label='ISBN'>
                        <select
                            className='select'
                            data-testid='reservation-isbn'
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
                    <Field label='Member'>
                        <select
                            className='select'
                            data-testid='reservation-member'
                            value={memberId}
                            onChange={(event) => setMemberId(event.target.value)}>
                            <option value=''>Select a member</option>
                            {members.items.map((member) => (
                                <option key={member.memberId} value={member.memberId}>
                                    {member.firstName} {member.lastName}
                                </option>
                            ))}
                        </select>
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='reservation-submit'
                        disabled={rejection.isPerforming}>
                        Reserve
                    </button>
                </form>
            </Card>

            <TableCard testId='reservations-table'>
                <thead>
                    <tr>
                        <th>ISBN</th>
                        <th>Title</th>
                        <th>Member</th>
                        <th>Reserved on</th>
                    </tr>
                </thead>
                <tbody>
                    {reservations.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={reservations.isLoading}
                            message='No reservations yet.'
                        />
                    ) : (
                        reservations.items.map((reservation) => (
                            <tr
                                key={reservation.reservationId}
                                data-testid='reservation-row'
                                data-id={reservation.reservationId}>
                                <td>{reservation.isbn}</td>
                                <td>{reservation.title}</td>
                                <td>{reservation.memberName}</td>
                                <td>{formatDateTime(reservation.reservedOn)}</td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
