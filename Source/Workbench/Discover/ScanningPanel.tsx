// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { FormEvent, useState } from 'react';
import { Button, Chip, EmptyState, Panel, ProviderMark, SegmentedControl } from '../Components';
import { hostInfo } from '../Systems/hosts';
import { RepositoryHost } from '../Systems/RepositoryHost';
import { AnalysisState } from './AnalysisState';
import { AnalysisStatus } from './Map/AnalysisStatus';
import { fileMarker, isActive, standing } from './discoverState';

export interface ScanningPanelProps {
    readonly statuses: readonly AnalysisStatus[];
    readonly busy: boolean;
    readonly onAnalyze: (status: AnalysisStatus) => void;
    readonly onAnalyzeFolder: (folder: string) => void;
}

const stateChip = (status: AnalysisStatus) => {
    switch (status.state) {
        case AnalysisState.complete: return <Chip variant="ok">Analyzed</Chip>;
        case AnalysisState.failed: return <span title={status.reason}><Chip variant="bad">Could not finish</Chip></span>;
        case AnalysisState.running: return <Chip variant="http">Scanning</Chip>;
        case AnalysisState.queued: return <Chip>Queued</Chip>;
        default: return <Chip variant="warn">Not analyzed</Chip>;
    }
};

/** The analysis of each system: its repository, branch, sub path and commit, the files being read, and a form to analyze any folder. */
export const ScanningPanel = ({ statuses, busy, onAnalyze, onAnalyzeFolder }: ScanningPanelProps) => {
    const [picked, setPicked] = useState('');
    const [folder, setFolder] = useState('');
    const selected = statuses.find(status => String(status.subject) === picked) ?? statuses[0];

    const submit = (event: FormEvent) => {
        event.preventDefault();
        if (folder.trim().length === 0) return;
        onAnalyzeFolder(folder.trim());
        setFolder('');
    };

    return (
        <Panel title="Scanning">
            {selected
                ? (
                    <>
                        {statuses.length > 1 && (
                            <SegmentedControl
                                label="System to show"
                                options={statuses.map(status => ({ value: String(status.subject), label: status.name }))}
                                value={String(selected.subject)}
                                onChange={setPicked} />
                        )}
                        <SourceLine status={selected} busy={busy} onAnalyze={() => onAnalyze(selected)} />
                        {selected.state === AnalysisState.failed && selected.reason && <p className="muted small-text note">{selected.reason}</p>}
                        <FileList status={selected} />
                    </>
                )
                : <EmptyState title="No code to scan" description="Say where a system's code lives under Systems, or analyze a folder below." />}
            <form className="folder-form" onSubmit={submit}>
                <input value={folder} onChange={event => setFolder(event.target.value)} placeholder="Or analyze a folder: /srv/legacy/library" aria-label="Folder to analyze" />
                <Button small type="submit" disabled={folder.trim().length === 0}>Analyze folder</Button>
            </form>
        </Panel>
    );
};

const SourceLine = ({ status, busy, onAnalyze }: { status: AnalysisStatus; busy: boolean; onAnalyze: () => void }) => {
    const info = hostInfo(status.host);
    const local = status.host === RepositoryHost.localFolder;

    return (
        <div className="srcline">
            <ProviderMark abbreviation={info.abbreviation} tone={info.tone} label={info.label} />
            <span className="mono">{status.repository}</span>
            {!local && <span className="muted">@ {status.branch}</span>}
            {status.subPath && <Chip>{status.subPath}</Chip>}
            {stateChip(status)}
            <span className="muted small-text push">{standing(status)}</span>
            <Button small disabled={busy || isActive(status.state)} onClick={onAnalyze}>{status.state === AnalysisState.notAnalyzed ? 'Analyze' : 'Analyze again'}</Button>
        </div>
    );
};

const FileList = ({ status }: { status: AnalysisStatus }) => {
    if (status.files.length === 0) return null;

    return (
        <div className="files" role="list" aria-label="Files being read">
            {status.files.map((file, index) => {
                const marker = fileMarker(index, status.filesRead, status.state);
                return (
                    <div key={file} role="listitem" className={`file ${marker === 'waiting' ? '' : marker}`}>
                        <span className="knob" aria-hidden="true" />
                        <span>{file}</span>
                        <span>{marker === 'done' ? '✓' : marker === 'current' ? 'reading' : ''}</span>
                    </div>
                );
            })}
            {status.totalFiles > status.files.length && <div className="file"><span /><span>{`and ${(status.totalFiles - status.files.length).toLocaleString('en-US')} more`}</span><span /></div>}
        </div>
    );
};
