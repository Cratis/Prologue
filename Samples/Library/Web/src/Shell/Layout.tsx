// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { NavLink, Outlet } from 'react-router-dom';
import { navigationItems } from './navigationItems';

/**
 * The chrome both frontends share — brand, navigation, and the badge telling a test which frontend it
 * is driving. Navigating here swaps the outlet without a page load, which is the visible difference
 * from the server-rendered Razor frontend.
 */
export const Layout = () => (
    <div className='app'>
        <header className='app-header'>
            <div className='brand'>Library</div>
            <nav className='nav'>
                {navigationItems.map((item) => (
                    <NavLink
                        key={item.path}
                        to={item.path}
                        data-testid={item.testId}
                        className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                        {item.label}
                    </NavLink>
                ))}
            </nav>
            <div className='badge' data-testid='frontend-kind'>
                React
            </div>
        </header>
        <Outlet />
    </div>
);
