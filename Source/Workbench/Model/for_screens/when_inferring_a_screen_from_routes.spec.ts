// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, expect, it } from 'vitest';
import { screenOfShape, screenOfShapes } from '../screens';

describe('when inferring a screen from routes', () => {
    it('names the first segment after api', () => expect(screenOfShape('POST /api/loans/{id}/return')).toBe('Loans'));
    it('title-cases a dashed segment', () => expect(screenOfShape('POST /api/book-tags')).toBe('Book Tags'));
    it('skips a version segment', () => expect(screenOfShape('POST /api/v2/members')).toBe('Members'));
    it('works without an api prefix', () => expect(screenOfShape('DELETE /reservations/{id}')).toBe('Reservations'));
    it('names nothing for a path of ids only', () => expect(screenOfShape('POST /api/{id}')).toBeUndefined());
    it('names nothing for the root', () => expect(screenOfShape('POST /')).toBeUndefined());
    it('picks the screen most shapes point at', () => expect(screenOfShapes(['POST /api/loans', 'POST /api/loans/{id}/return', 'POST /api/members'])).toBe('Loans'));
    it('picks the first when they tie', () => expect(screenOfShapes(['POST /api/members', 'POST /api/loans'])).toBe('Members'));
    it('names nothing without shapes', () => expect(screenOfShapes([])).toBeUndefined());
});
