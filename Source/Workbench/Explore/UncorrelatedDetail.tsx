// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Panel } from '../Components';
import { operationLetter } from '../Capture/format';
import { UncorrelatedLike } from './exploreTypes';
import { hourSummary } from './hours';

const hoursOfTheDay = Array.from({ length: 24 }, (_, hour) => hour);

/** Database writes that arrived with no HTTP command, per table and hour of the day. */
export const UncorrelatedDetail = ({ tables }: { tables: readonly UncorrelatedLike[] }) => {
    const total = tables.reduce((sum, table) => sum + table.transactions, 0);
    return (
        <Panel title="Shape detail" aside={`${total} transactions`}>
            <p>Database transactions that arrived with no HTTP command and no span in the correlation window. A scheduled job or direct SQL looks like this; Prologue cannot see which.</p>
            {tables.map(table => {
                const busiest = Math.max(1, ...table.hours.map(entry => entry.transactions));
                return (
                    <div className="uncorrelated" key={`${table.write.schema}.${table.write.table}`}>
                        <div>
                            <b>{table.write.table}</b> <span className="muted">{table.transactions} transactions</span>{' '}
                            {table.write.operations.map(({ operation }) => <span key={operation} className={`op ${operationLetter(operation)}`}>{operationLetter(operation)}</span>)}
                        </div>
                        <div className="mono muted">{table.write.columns.join(', ')}</div>
                        <div className="hours" role="img" aria-label={`Transactions per hour of the day, UTC. ${hourSummary(table.hours)}`}>
                            {hoursOfTheDay.map(hour => {
                                const count = table.hours.find(entry => entry.hour === hour)?.transactions ?? 0;
                                return <i key={hour} title={`${String(hour).padStart(2, '0')}:00 UTC · ${count}`} style={{ height: `${count === 0 ? 0 : Math.max(8, (count / busiest) * 100)}%` }} />;
                            })}
                        </div>
                        <div className="hours-axis muted"><span>00:00</span><span>12:00</span><span>23:00 UTC</span></div>
                        <div className="muted note">{hourSummary(table.hours)}</div>
                    </div>
                );
            })}
            <p className="muted note">This becomes a question for you in Events. If it is a reconciliation, it may deserve its own event.</p>
        </Panel>
    );
};
