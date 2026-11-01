// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export interface SegmentedOption<T extends string> {
    readonly value: T;
    readonly label: string;
}

export interface SegmentedControlProps<T extends string> {
    readonly label: string;
    readonly options: readonly SegmentedOption<T>[];
    readonly value: T;
    readonly onChange: (value: T) => void;
}

export const SegmentedControl = <T extends string>({ label, options, value, onChange }: SegmentedControlProps<T>) => (
    <div className="seg" role="group" aria-label={label}>
        {options.map(option => (
            <button key={option.value} type="button" aria-pressed={option.value === value} onClick={() => onChange(option.value)}>
                {option.label}
            </button>
        ))}
    </div>
);
