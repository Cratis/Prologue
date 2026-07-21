// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The props for {@link EmptyRow}.
 */
export interface EmptyRowProps {
    /** How many columns the table has, so the message spans all of them. */
    columnCount: number;

    /** Whether the collection is still loading. */
    isLoading: boolean;

    /** What to say when the collection loaded and came back empty. */
    message: string;
}

/**
 * The row a table shows while loading, and when there is nothing to show.
 * @param props The {@link EmptyRowProps}.
 */
export const EmptyRow = ({ columnCount, isLoading, message }: EmptyRowProps) => (
    <tr>
        <td className='empty' colSpan={columnCount}>
            {isLoading ? 'Loading' : message}
        </td>
    </tr>
);
