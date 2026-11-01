// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ExtractedSliceType } from '../../ExtractedSliceType';
import { AcceptedModel } from '../../Events/Accepted/AcceptedModel';

const event = (name: string, stream: string, isOpen = false) => ({ key: `m/${name}`, name, mechanical: name, stream, properties: [], confidence: 0.9, isOpen });

const slice = (name: string, type: ExtractedSliceType, shapes: string[], events: ReturnType<typeof event>[], commands: string[] = [], readModels: string[] = []) => ({
    name, type, description: '', shapes, commands: commands.map(command => ({ name: command, properties: [], rules: [], description: '' })), events,
    readModels: readModels.map(readModel => ({ name: readModel, properties: [] })), projections: [], constraints: [],
    rules: [{ kind: 0, text: `${name} rule`, shape: shapes[0] ?? '' }], isOpen: events.some(item => item.isOpen)
});

/** A small library: Authors with one accepted slice, Lending with an accepted and an open slice and a read model slice. */
export const library = {
    id: '00000000-0000-0000-0000-000000000001',
    systemName: 'Library',
    accepted: 2,
    open: 1,
    rejected: 0,
    openBoundaries: 0,
    rules: [],
    modules: [
        { name: 'Authors', features: [{ name: 'Authors', slices: [slice('RegisterAuthor', ExtractedSliceType.stateChange, ['POST /api/authors'], [event('AuthorRegistered', 'Author')], ['RegisterAuthor'])] }] },
        {
            name: 'Lending',
            features: [{
                name: 'Loans',
                slices: [
                    slice('LendBook', ExtractedSliceType.stateChange, ['POST /api/loans'], [event('BookLent', 'Loan')], ['LendBook']),
                    slice('ReturnBook', ExtractedSliceType.stateChange, ['POST /api/loans/{id}/return'], [event('BookReturned', 'Loan', true), event('CopyRestocked', 'Inventory')], ['ReturnBook']),
                    slice('ActiveLoans', ExtractedSliceType.stateView, [], [], [], ['ActiveLoans'])
                ]
            }]
        }
    ]
} as unknown as AcceptedModel;
