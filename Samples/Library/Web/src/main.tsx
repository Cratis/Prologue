// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createRoot } from 'react-dom/client';
import { App } from './App';

// The one stylesheet both frontends share, imported straight out of ../../Shared rather than copied —
// Vite's server.fs.allow is widened to reach across the project boundary.
import '../../Shared/library.css';

const container = document.getElementById('root');

if (!container) {
    throw new Error('The root element is missing from index.html');
}

// Deliberately not wrapped in StrictMode: its double-invoked effects would fire every load twice, and
// the point of this sample is the HTTP shape the Prologue Extractor captures.
createRoot(container).render(<App />);
