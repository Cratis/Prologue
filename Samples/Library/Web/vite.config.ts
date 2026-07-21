// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// Aspire's AddViteApp hands the port down through the PORT environment variable (and repeats it as
// --port on the command line), so the dev server has to honor it rather than insisting on 5173.
const port = Number(process.env.PORT) || 5173;

export default defineConfig({
    plugins: [react()],
    server: {
        port,
        // The one stylesheet both frontends share lives outside this project, in ../Shared. Vite refuses
        // to serve files from outside the project root unless they are on the allow list.
        fs: {
            allow: ['..', '../Shared']
        }
    },
    preview: {
        port
    }
});
