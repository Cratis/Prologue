// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { boardSummary, eventsOfStream, layoutBoard } from '../board';
import { library } from './fixtures';

describe('when laying out the board', () => {
    const board = layoutBoard(library);
    const lending = board.modules[1];

    it('groups the columns under their modules in model order', () => expect(board.modules.map(module => [module.name, module.slices.length])).toEqual([['Authors', 1], ['Lending', 3]]));
    it('makes one column per slice', () => expect(board.slices.map(slice => slice.name)).toEqual(['RegisterAuthor', 'LendBook', 'ReturnBook', 'ActiveLoans']));
    it('gives each slice a key that tells the modules apart', () => expect(new Set(board.slices.map(slice => slice.key)).size).toBe(4));
    it('makes one event row per stream in the order they first appear', () => expect(board.streams).toEqual(['Author', 'Loan', 'Inventory']));
    it('puts only the events of a stream in its row', () => expect(eventsOfStream(lending.slices[1], 'Loan').map(item => item.name)).toEqual(['BookReturned']));
    it('leaves a row empty for a slice with no event on that stream', () => expect(eventsOfStream(lending.slices[0], 'Inventory')).toEqual([]));
    it('infers the screen of a slice from its route', () => expect(lending.slices[1].screen).toBe('Loans'));
    it('infers no screen for a slice without shapes', () => expect(lending.slices[2].screen).toBeUndefined());
    it('lists the read models of a view slice', () => expect(lending.slices[2].readModels).toEqual(['ActiveLoans']));
    it('marks a slice with an open event as open', () => expect(lending.slices.map(slice => slice.isOpen)).toEqual([false, true, false]));
    it('summarizes the board', () => expect(boardSummary(board)).toBe('4 slices · 3 accepted events · 1 open'));
    it('is empty before the model arrives', () => expect(layoutBoard(undefined).slices).toEqual([]));
});
