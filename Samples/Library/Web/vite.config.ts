// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// Aspire's AddViteApp hands the port down through the PORT environment variable (and repeats it as
// --port on the command line), so the dev server has to honor it rather than insisting on 5173.
const port = Number(process.env.PORT) || 5173;

// Where the API lives. The Aspire composition points this at the Prologue Extractor's reverse proxy, so the SPA's
// calls are captured as HTTP commands exactly as the server-rendered frontend's form posts are.
const api = process.env.VITE_API_BASE_URL ?? 'http://localhost:5280';

export default defineConfig({
    plugins: [react()],
    server: {
        port,

        // The app calls /api relatively and Vite forwards it, so the browser only ever talks to its own origin.
        // Calling the proxy cross-origin instead would mean a preflight on every write, and the proxy answers
        // OPTIONS with 404 — reads would work and writes would silently fail.
        proxy: {
            '/api': {
                target: api,
                changeOrigin: true
            }
        },
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
