// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { Button, Chip, CopyButton } from '../Components';
import { sharePath } from './discoverState';
import { ObserveSharedMap } from './Sharing/SharedMap';
import { useDiscoverActions } from './useDiscoverActions';

export interface ShareMapControlProps {
    readonly prologue: Guid;
    readonly actions: Pick<ReturnType<typeof useDiscoverActions>, 'share' | 'stopSharing' | 'isBusy'>;
}

/** Shares the map through a read-only link, shows the link once it is shared, and stops sharing it. */
export const ShareMapControl = ({ prologue, actions }: ShareMapControlProps) => {
    const [shares] = ObserveSharedMap.use({ prologue });
    const shared = shares.data.find(share => share.isShared);

    if (!shared) return <Button disabled={actions.isBusy('share-map')} onClick={() => void actions.share()}>Share map</Button>;

    const link = `${window.location.origin}${sharePath(shared.token)}`;
    return (
        <span className="row share">
            <Chip variant="ok">Shared read-only</Chip>
            <a className="mono small-text" href={sharePath(shared.token)} target="_blank" rel="noreferrer">{link}</a>
            <CopyButton text={link} label="Copy link" />
            <Button variant="ghost" disabled={actions.isBusy('stop-sharing-map')} onClick={() => void actions.stopSharing()}>Stop sharing</Button>
        </span>
    );
};
