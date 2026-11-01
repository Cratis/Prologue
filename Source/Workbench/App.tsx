// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Arc } from '@cratis/arc.react';
import { ToastProvider } from './Components';
import { shareTokenFromPath } from './Discover/discoverState';
import { SharedMapPage } from './Discover/SharedMapPage';
import { RouterProvider, useRouter } from './Shell/Router';
import { Shell } from './Shell/Shell';
import { ShellStateProvider } from './Shell/ShellStateProvider';

/** A shared map opens on its own, with no shell around it; every other path is the Workbench. */
const Routes = () => {
    const { path } = useRouter();
    const token = shareTokenFromPath(path);

    return token
        ? <SharedMapPage token={token} />
        : <ShellStateProvider><Shell /></ShellStateProvider>;
};

export const App = () => (
    <Arc development={import.meta.env.DEV}>
        <ToastProvider>
            <RouterProvider>
                <Routes />
            </RouterProvider>
        </ToastProvider>
    </Arc>
);
