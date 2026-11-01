// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ButtonHTMLAttributes } from 'react';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    readonly variant?: 'default' | 'primary' | 'ghost';
    readonly small?: boolean;
}

export const Button = ({ variant = 'default', small = false, className, type = 'button', ...rest }: ButtonProps) => {
    const classes = ['btn', variant === 'default' ? '' : variant, small ? 'small' : '', className ?? ''].filter(Boolean).join(' ');
    return <button type={type} className={classes} {...rest} />;
};
