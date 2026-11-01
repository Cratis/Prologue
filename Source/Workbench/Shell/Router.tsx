// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, MouseEvent, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';

interface RouterValue {
    readonly path: string;
    readonly navigate: (path: string) => void;
}

const RouterContext = createContext<RouterValue>({ path: '/', navigate: () => undefined });

/** Client-side routing on the History API; the Receiver answers every non-file path with index.html. */
export const RouterProvider = ({ children }: { children: ReactNode }) => {
    const [path, setPath] = useState(() => window.location.pathname);

    useEffect(() => {
        const onPop = () => setPath(window.location.pathname);
        window.addEventListener('popstate', onPop);
        return () => window.removeEventListener('popstate', onPop);
    }, []);

    const navigate = useCallback((target: string) => {
        if (target === window.location.pathname) return;
        window.history.pushState({}, '', target);
        setPath(target);
        document.querySelector('main')?.scrollTo({ top: 0 });
    }, []);

    const value = useMemo(() => ({ path, navigate }), [path, navigate]);
    return <RouterContext.Provider value={value}>{children}</RouterContext.Provider>;
};

export const useRouter = () => useContext(RouterContext);

export interface LinkProps {
    readonly to: string;
    readonly className?: string;
    readonly children: ReactNode;
    readonly 'aria-current'?: 'page';
}

/** A link that navigates without reloading, while staying a real anchor for open-in-new-tab and assistive technology. */
export const Link = ({ to, className, children, ...rest }: LinkProps) => {
    const { navigate } = useRouter();
    const onClick = (event: MouseEvent<HTMLAnchorElement>) => {
        if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        navigate(to);
    };
    return <a href={to} className={className} onClick={onClick} {...rest}>{children}</a>;
};
