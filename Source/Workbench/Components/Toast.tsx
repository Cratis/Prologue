// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, ReactNode, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';

const visibleForMilliseconds = 2400;

interface ToastApi {
    /** Shows a short message for a moment. */
    readonly show: (message: string) => void;
}

const ToastContext = createContext<ToastApi>({ show: () => undefined });

export const ToastProvider = ({ children }: { children: ReactNode }) => {
    const [message, setMessage] = useState('');
    const timer = useRef<number>(undefined);

    const show = useCallback((text: string) => {
        window.clearTimeout(timer.current);
        setMessage(text);
        timer.current = window.setTimeout(() => setMessage(''), visibleForMilliseconds);
    }, []);

    useEffect(() => () => window.clearTimeout(timer.current), []);

    const api = useMemo(() => ({ show }), [show]);

    return (
        <ToastContext.Provider value={api}>
            {children}
            <div role="status" aria-live="polite">
                {message && <div className="toast">{message}</div>}
            </div>
        </ToastContext.Provider>
    );
};

export const useToast = () => useContext(ToastContext);
