// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { EmitMetadataPlugin } from '@cratis/arc.vite';

// The Receiver serves what is built here from its wwwroot, with an SPA fallback.
const receiver = fileURLToPath(new URL('../Receiver', import.meta.url));
const backend = 'http://localhost:5000';

export default defineConfig({
    root: fileURLToPath(new URL('./', import.meta.url)),
    build: {
        outDir: `${receiver}/wwwroot`,
        emptyOutDir: true,
        modulePreload: false,
        target: 'esnext'
    },
    plugins: [
        react(),
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        EmitMetadataPlugin({ tsconfigPath: fileURLToPath(new URL('./tsconfig.json', import.meta.url)) }) as any
    ],
    resolve: {
        dedupe: ['@cratis/fundamentals', 'react', 'react-dom', 'react/jsx-runtime', 'react/jsx-dev-runtime']
    },
    server: {
        port: 9200,
        host: true,
        proxy: {
            '/.cratis': { target: backend, ws: true },
            '/api': { target: backend, ws: true },
            '/captures': { target: backend },
            '/prologues': { target: backend }
        }
    }
});
