// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import {
    AIProvider, AIProviderType,
    AddAnthropicProvider, AddAzureOpenAIProvider, AddCopilotProvider, AddOpenAICodexProvider,
    AddOpenAICompatibleProvider, AddOpenAIProvider, AddZAIProvider,
    DisconnectCopilotProvider, DisconnectOpenAICodexProvider,
    StartCopilotSignIn, StartOpenAISubscriptionSignIn
} from '@cratis/ai';
import { Guid } from '@cratis/fundamentals';
import { Runnable } from '../../Shell/useCommandRunner';

/** The values every vendor's add command shares, whatever else it carries. */
export interface AddProviderValues {
    name?: string;
    endpoint?: string;
    apiKey?: string;
    maxConcurrentJobs?: number;
}

/** A vendor's add command as the add form drives it. */
export type AddProviderCommand = AddProviderValues & Runnable;

/** A command that acts on one provider. */
export type ProviderCommand = Runnable & { provider?: Guid };

/** What a device sign-in vendor needs: the commands behind Connect and Disconnect, and who the person signs in to. */
export interface DeviceSignIn {
    readonly start: new () => ProviderCommand;
    readonly disconnect: new () => ProviderCommand;
    readonly account: string;
}

/** Everything the panel knows about one vendor; adding a vendor means adding one entry here. */
export interface Vendor {
    readonly type: AIProviderType;
    readonly label: string;

    /** The two letters on the vendor's mark. */
    readonly mark: string;
    readonly add: new () => AddProviderCommand;
    readonly endpoint?: { readonly placeholder: string; readonly defaultValue?: string };
    readonly apiKey?: { readonly placeholder: string };

    /** Said instead of a key field, for vendors that connect through a sign-in. */
    readonly note?: string;
    readonly deviceSignIn?: DeviceSignIn;
}

const commandOf = <T,>(command: unknown) => command as new () => T;

/** The vendors the form offers, in the order the picker lists them. */
export const vendors: readonly Vendor[] = [
    { type: AIProviderType.anthropic, label: 'Anthropic', mark: 'An', add: commandOf(AddAnthropicProvider), apiKey: { placeholder: 'sk-ant-...' } },
    { type: AIProviderType.openAI, label: 'OpenAI', mark: 'OA', add: commandOf(AddOpenAIProvider), apiKey: { placeholder: 'sk-...' } },
    {
        type: AIProviderType.azureOpenAI, label: 'Azure OpenAI', mark: 'Az', add: commandOf(AddAzureOpenAIProvider),
        endpoint: { placeholder: 'https://<resource>.openai.azure.com' }, apiKey: { placeholder: 'The resource key' }
    },
    {
        type: AIProviderType.openAICompatible, label: 'OpenAI-compatible', mark: 'OC', add: commandOf(AddOpenAICompatibleProvider),
        endpoint: { placeholder: 'https://host/v1' }, apiKey: { placeholder: 'Optional - some endpoints run without one' },
        note: 'Use this for models in your network: Ollama, vLLM or LM Studio. An endpoint at a private address is marked as inside your network.'
    },
    {
        type: AIProviderType.ZAI, label: 'Z.ai', mark: 'Z', add: commandOf(AddZAIProvider),
        endpoint: { placeholder: 'https://api.z.ai/api/anthropic', defaultValue: 'https://api.z.ai/api/anthropic' }, apiKey: { placeholder: 'The Z.ai API key' }
    },
    {
        type: AIProviderType.openAICodex, label: 'OpenAI Codex', mark: 'Cx', add: commandOf(AddOpenAICodexProvider),
        note: 'Add the provider, then use Connect on its row to authenticate with ChatGPT.',
        deviceSignIn: { start: commandOf(StartOpenAISubscriptionSignIn), disconnect: commandOf(DisconnectOpenAICodexProvider), account: 'ChatGPT' }
    },
    {
        type: AIProviderType.copilot, label: 'GitHub Copilot', mark: 'Co', add: commandOf(AddCopilotProvider),
        note: 'Add the provider, then use Connect on its row to authenticate with GitHub.',
        deviceSignIn: { start: commandOf(StartCopilotSignIn), disconnect: commandOf(DisconnectCopilotProvider), account: 'GitHub' }
    }
];

/**
 * Finds a vendor.
 * @param type The vendor type.
 * @returns The vendor, or undefined for a type the panel does not add.
 */
export const vendorOf = (type: AIProviderType): Vendor | undefined => vendors.find(vendor => vendor.type === type);

/**
 * Whether a provider is connected through a device sign-in.
 * @param provider The provider.
 * @returns True when it has a connected subscription.
 */
export const isConnected = (provider: AIProvider): boolean => provider.isSubscription;
