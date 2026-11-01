// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useState } from 'react';

/**
 * Freezes what the feed shows without disconnecting from the server: while paused, the view is the data as it was when pause was pressed
 * and new captures keep arriving in the numbers behind it.
 * @param live The latest data.
 * @returns The data to show, whether the feed is paused, and the means to pause and resume.
 */
export const useFeedPause = <T,>(live: T) => {
    const [held, setHeld] = useState<{ readonly value: T } | undefined>();
    const pause = useCallback(() => setHeld({ value: live }), [live]);
    const resume = useCallback(() => setHeld(undefined), []);
    return { view: held ? held.value : live, paused: held !== undefined, pause, resume };
};
