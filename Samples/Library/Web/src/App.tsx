// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { Authors } from './Authors';
import { Catalog } from './Catalog';
import { Inventory } from './Inventory';
import { Loans } from './Loans';
import { Members } from './Members';
import { Reservations } from './Reservations';
import { Layout } from './Shell';
import { Simulation } from './Simulation';

/**
 * The routes, all sharing the header. Navigating between them swaps the outlet client-side — no page
 * load — which is what makes this frontend's HTTP shape differ from the server-rendered one.
 */
export const App = () => (
    <BrowserRouter>
        <Routes>
            <Route element={<Layout />}>
                <Route index element={<Navigate to='/authors' replace />} />
                <Route path='authors' element={<Authors />} />
                <Route path='members' element={<Members />} />
                <Route path='catalog' element={<Catalog />} />
                <Route path='inventory' element={<Inventory />} />
                <Route path='reservations' element={<Reservations />} />
                <Route path='loans' element={<Loans />} />
                <Route path='simulation' element={<Simulation />} />
                <Route path='*' element={<Navigate to='/authors' replace />} />
            </Route>
        </Routes>
    </BrowserRouter>
);
