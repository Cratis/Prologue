// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Chip, EmptyState, Panel } from '../Components';
import { Link } from '../Shell/Router';
import { DriftItem } from './Drift/DriftItem';
import { driftRows } from './drift';

export interface DriftPanelProps {
    readonly accepted: boolean;
    readonly items: readonly DriftItem[];
    readonly isBusy: (key: string) => boolean;
    readonly onAcceptModel: () => void;
    readonly onSuggest: (key: string) => void;
}

/** What the legacy system does that the accepted model does not cover: commands first seen since, and columns a known command newly writes. */
export const DriftPanel = ({ accepted, items, isBusy, onAcceptModel, onSuggest }: DriftPanelProps) => {
    const rows = driftRows(items);

    return (
        <Panel title="The legacy system keeps changing" aside="Prologue keeps capturing after the model is accepted">
            {!accepted
                ? (
                    <EmptyState
                        title="The model is not accepted yet"
                        description="Once it is, whatever the legacy system did until now is what the model covers. Commands first seen after that, and columns a known command newly writes, show up here."
                        action={<Button variant="primary" disabled={isBusy('accept-model')} onClick={onAcceptModel}>Mark the model accepted</Button>} />
                )
                : rows.length === 0
                    ? <EmptyState title="No drift" description="Every command and column captured since the model was accepted is already covered." />
                    : (
                        <div className="scroll-x">
                            <table className="data">
                                <caption className="visually-hidden">Seen since the model was accepted</caption>
                                <thead><tr><th scope="col">Seen since the model was accepted</th><th scope="col">Captures</th><th scope="col">In the model?</th><th scope="col"><span className="visually-hidden">Action</span></th></tr></thead>
                                <tbody>
                                    {rows.map(row => (
                                        <tr key={row.key}>
                                            <td className="mono">{row.text}</td>
                                            <td className="num">{row.captures}</td>
                                            <td><Chip variant={row.variant}>{row.status}</Chip></td>
                                            <td>
                                                {row.canSuggest
                                                    ? <Button small disabled={isBusy(`drift-${row.key}`)} onClick={() => onSuggest(row.key)}>Suggest events</Button>
                                                    : <Link to="/events" className="btn small">Open Events</Link>}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
        </Panel>
    );
};
