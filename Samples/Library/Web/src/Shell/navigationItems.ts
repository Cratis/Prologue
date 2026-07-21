// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { NavigationItem } from './NavigationItem';

/**
 * The header navigation, in the order both frontends render it.
 */
export const navigationItems: NavigationItem[] = [
    { path: '/authors', label: 'Authors', testId: 'nav-authors' },
    { path: '/members', label: 'Members', testId: 'nav-members' },
    { path: '/catalog', label: 'Catalog', testId: 'nav-catalog' },
    { path: '/inventory', label: 'Inventory', testId: 'nav-inventory' },
    { path: '/reservations', label: 'Reservations', testId: 'nav-reservations' },
    { path: '/loans', label: 'Loans', testId: 'nav-loans' },
    { path: '/simulation', label: 'Simulation', testId: 'nav-simulation' }
];
