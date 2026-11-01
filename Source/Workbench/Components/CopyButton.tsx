// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from './Button';
import { useToast } from './Toast';

export interface CopyButtonProps {
    readonly text: string;
    readonly label?: string;
}

/** Copies a text to the clipboard and says so; when the browser refuses it points at selecting the text instead. */
export const CopyButton = ({ text, label = 'Copy' }: CopyButtonProps) => {
    const toast = useToast();

    const copy = async () => {
        try {
            await navigator.clipboard.writeText(text);
            toast.show('Copied to clipboard');
        } catch {
            toast.show('Copy was blocked by the browser. Select the text instead.');
        }
    };

    return <Button onClick={copy}>{label}</Button>;
};
