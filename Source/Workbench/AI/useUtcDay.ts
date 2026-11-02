// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useEffect, useState } from 'react';
import { millisecondsToNextUtcDay, utcDay } from './poolUsage';

/**
 * The current UTC day, which changes on its own at UTC midnight so what is counted "today" does not go stale on a page left open.
 * @returns The day, as yyyy-MM-dd.
 */
export const useUtcDay = (): string => {
    const [day, setDay] = useState(() => utcDay(new Date()));

    useEffect(() => {
        const timer = setTimeout(() => setDay(utcDay(new Date())), millisecondsToNextUtcDay(new Date()) + 50);
        return () => clearTimeout(timer);
    }, [day]);

    return day;
};
