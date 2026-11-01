// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { AnalysisState } from '../AnalysisState';
import { fileMarker, sharePath, shareTokenFromPath, standing } from '../discoverState';

describe('when saying where an analysis stands', () => {
    it('shows progress while it runs', () => expect(standing({ state: AnalysisState.running, filesRead: 3, totalFiles: 10, commit: undefined })).toBe('scanning 3 of 10'));
    it('shows the short commit when it is complete', () => expect(standing({ state: AnalysisState.complete, filesRead: 10, totalFiles: 10, commit: 'a41c9e2f0b' })).toBe('analyzed a41c9e2'));
    it('says it is waiting when queued', () => expect(standing({ state: AnalysisState.queued, filesRead: 0, totalFiles: 0, commit: undefined })).toBe('waiting for its turn'));
    it('says it could not finish when it failed', () => expect(standing({ state: AnalysisState.failed, filesRead: 0, totalFiles: 0, commit: undefined })).toBe('could not finish'));
});

describe('when marking the files of an analysis', () => {
    it('marks files before the position as done', () => expect(fileMarker(1, 3, AnalysisState.running)).toBe('done'));
    it('marks the file at the position as current while running', () => expect(fileMarker(3, 3, AnalysisState.running)).toBe('current'));
    it('marks later files as waiting', () => expect(fileMarker(5, 3, AnalysisState.running)).toBe('waiting'));
    it('marks every file done when complete', () => expect(fileMarker(9, 0, AnalysisState.complete)).toBe('done'));
});

describe('when reading a share link', () => {
    it('finds the token in a share path', () => expect(shareTokenFromPath('/share/abc-DEF_123')).toBe('abc-DEF_123'));
    it('ignores other paths', () => expect(shareTokenFromPath('/discover')).toBeUndefined());
    it('ignores a share path with no token', () => expect(shareTokenFromPath('/share/')).toBeUndefined());
    it('round trips a token through its path', () => expect(shareTokenFromPath(sharePath('a/b c'))).toBe('a/b c'));
});
