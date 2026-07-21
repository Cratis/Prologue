// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import type { BookDetails } from '../Catalog';
import type { MemberDetails } from '../Members';
import { Card, EmptyRow, Field, formatDateTime, Page, TableCard } from '../Shell';
import type { LoanDetails } from './LoanDetails';

const columnCount = 6;

/**
 * Which copies are out with which members. Lending a title with nothing on the shelf is rejected with
 * 422, and returning a loan twice with 409.
 */
export const Loans = () => {
    const loans = useCollection<LoanDetails>('/api/loans');
    const books = useCollection<BookDetails>('/api/catalog/books');
    const members = useCollection<MemberDetails>('/api/members');
    const rejection = useRejection();
    const [isbn, setIsbn] = useState('');
    const [memberId, setMemberId] = useState('');

    const lend = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<LoanDetails>('/api/loans', {
                isbn,
                memberId: Number(memberId)
            });

            await loans.reload();
        });
    };

    const returnLoan = (loan: LoanDetails) =>
        rejection.perform(async () => {
            await apiClient.post<LoanDetails>(`/api/loans/${loan.loanId}/return`);
            await loans.reload();
        });

    return (
        <Page
            title='Loans'
            subtitle='Which copies are out, and with whom.'
            problem={rejection.problem ?? loans.problem ?? books.problem ?? members.problem}>
            <Card title='Lend a copy'>
                <form className='form-row' data-testid='loan-form' onSubmit={lend}>
                    <Field label='ISBN'>
                        <select
                            className='select'
                            data-testid='loan-isbn'
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
                            data-testid='loan-member'
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
                        data-testid='loan-submit'
                        disabled={rejection.isPerforming}>
                        Lend
                    </button>
                </form>
            </Card>

            <TableCard testId='loans-table'>
                <thead>
                    <tr>
                        <th>ISBN</th>
                        <th>Title</th>
                        <th>Member</th>
                        <th>Lent on</th>
                        <th>Returned on</th>
                        <th />
                    </tr>
                </thead>
                <tbody>
                    {loans.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={loans.isLoading}
                            message='Nothing is out on loan.'
                        />
                    ) : (
                        loans.items.map((loan) => (
                            <tr key={loan.loanId} data-testid='loan-row' data-id={loan.loanId}>
                                <td>{loan.isbn}</td>
                                <td>{loan.title}</td>
                                <td>{loan.memberName}</td>
                                <td>{formatDateTime(loan.lentOn)}</td>
                                <td>{formatDateTime(loan.returnedOn)}</td>
                                <td>
                                    {loan.returnedOn === null && (
                                        <button
                                            className='button button-secondary'
                                            type='button'
                                            data-testid='loan-return'
                                            disabled={rejection.isPerforming}
                                            onClick={() => void returnLoan(loan)}>
                                            Return
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
