// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Arc } from '@cratis/arc.react';
import { ToastProvider } from './Components';
import { RouterProvider } from './Shell/Router';
import { Shell } from './Shell/Shell';
import { ShellStateProvider } from './Shell/ShellStateProvider';

export const App = () => (
    <Arc development={import.meta.env.DEV}>
        <ToastProvider>
            <ShellStateProvider>
                <RouterProvider>
                    <Shell />
                </RouterProvider>
            </ShellStateProvider>
        </ToastProvider>
    </Arc>
);
