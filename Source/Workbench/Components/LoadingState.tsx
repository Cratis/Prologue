// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface LoadingStateProps {
    readonly label?: string;
}

export const LoadingState = ({ label = 'Loading' }: LoadingStateProps) => (
    <div className="state" role="status">
        <span className="spinner" aria-hidden="true" />
        <span>{label}</span>
    </div>
);
