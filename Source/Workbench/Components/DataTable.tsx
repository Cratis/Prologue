// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';
import { EmptyState } from './EmptyState';

export interface DataColumn<T> {
    readonly key: string;
    readonly header: string;
    readonly render: (row: T) => ReactNode;
}

export interface DataTableProps<T> {
    readonly columns: readonly DataColumn<T>[];
    readonly rows: readonly T[];
    readonly rowKey: (row: T) => string;
    readonly caption: string;
    readonly selectedKey?: string;
    readonly onRowClick?: (row: T) => void;
    readonly emptyTitle?: string;
}

export const DataTable = <T,>({ columns, rows, rowKey, caption, selectedKey, onRowClick, emptyTitle = 'Nothing here yet' }: DataTableProps<T>) => {
    if (rows.length === 0) return <EmptyState title={emptyTitle} />;

    return (
        <div className="scroll-x">
            <table className="data">
                <caption className="visually-hidden">{caption}</caption>
                <thead>
                    <tr>{columns.map(column => <th key={column.key} scope="col">{column.header}</th>)}</tr>
                </thead>
                <tbody>
                    {rows.map(row => {
                        const key = rowKey(row);
                        const classes = [onRowClick ? 'click' : '', key === selectedKey ? 'selected' : ''].filter(Boolean).join(' ');
                        return (
                            <tr key={key} className={classes || undefined} onClick={onRowClick && (() => onRowClick(row))}>
                                {columns.map(column => <td key={column.key}>{column.render(row)}</td>)}
                            </tr>
                        );
                    })}
                </tbody>
            </table>
        </div>
    );
};
