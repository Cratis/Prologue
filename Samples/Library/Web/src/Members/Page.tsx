// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState, type FormEvent } from 'react';
import { apiClient, useCollection, useRejection } from '../Api';
import { Card, EmptyRow, Field, Page, TableCard } from '../Shell';
import type { MemberDetails } from './MemberDetails';

const columnCount = 3;

/**
 * The people registered with the library.
 */
export const Members = () => {
    const members = useCollection<MemberDetails>('/api/members');
    const rejection = useRejection();
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');

    const register = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        await rejection.perform(async () => {
            await apiClient.post<MemberDetails>('/api/members', { firstName, lastName });
            setFirstName('');
            setLastName('');
            await members.reload();
        });
    };

    return (
        <Page
            title='Members'
            subtitle='The people registered with the library.'
            problem={rejection.problem ?? members.problem}>
            <Card title='Register member'>
                <form className='form-row' data-testid='member-form' onSubmit={register}>
                    <Field label='First name'>
                        <input
                            className='input'
                            data-testid='member-first-name'
                            value={firstName}
                            onChange={(event) => setFirstName(event.target.value)}
                        />
                    </Field>
                    <Field label='Last name'>
                        <input
                            className='input'
                            data-testid='member-last-name'
                            value={lastName}
                            onChange={(event) => setLastName(event.target.value)}
                        />
                    </Field>
                    <button
                        className='button'
                        type='submit'
                        data-testid='member-submit'
                        disabled={rejection.isPerforming}>
                        Register
                    </button>
                </form>
            </Card>

            <TableCard testId='members-table'>
                <thead>
                    <tr>
                        <th className='numeric'>Id</th>
                        <th>First name</th>
                        <th>Last name</th>
                    </tr>
                </thead>
                <tbody>
                    {members.items.length === 0 ? (
                        <EmptyRow
                            columnCount={columnCount}
                            isLoading={members.isLoading}
                            message='No members registered yet.'
                        />
                    ) : (
                        members.items.map((member) => (
                            <tr
                                key={member.memberId}
                                data-testid='member-row'
                                data-id={member.memberId}>
                                <td className='numeric'>{member.memberId}</td>
                                <td>{member.firstName}</td>
                                <td>{member.lastName}</td>
                            </tr>
                        ))
                    )}
                </tbody>
            </TableCard>
        </Page>
    );
};
