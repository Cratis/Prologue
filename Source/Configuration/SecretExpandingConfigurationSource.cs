// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Cratis.Prologue.Configuration;

/// <summary>
/// A configuration source that overrides every value holding a <c language="text">${secret:&lt;name&gt;}</c> placeholder
/// with the expanded value. It reads the sources added before it, so it is added last.
/// </summary>
/// <param name="previous">The sources the values are read from.</param>
sealed class SecretExpandingConfigurationSource(IReadOnlyList<IConfigurationSource> previous) : IConfigurationSource
{
    /// <inheritdoc/>
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(previous);

    sealed class Provider(IReadOnlyList<IConfigurationSource> previous) : ConfigurationProvider
    {
        IConfigurationRoot? _root;
        IDisposable? _registration;

        public override void Load()
        {
            _root ??= new ConfigurationRoot([.. previous.Select(source => source.Build(new ConfigurationBuilder()))]);
            _registration ??= ChangeToken.OnChange(_root.GetReloadToken, Reload);
            Data = Expand(_root);
        }

        static Dictionary<string, string?> Expand(IConfigurationRoot root)
        {
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in root.AsEnumerable())
            {
                if (SecretPlaceholders.HasPlaceholder(value))
                {
                    data[key] = SecretPlaceholders.Expand(root, value!);
                }
            }

            return data;
        }

        void Reload()
        {
            try
            {
                Data = Expand(_root!);
                OnReload();
            }
            catch (SecretNotConfigured)
            {
                // A reload that cannot resolve a secret keeps the last good values rather than taking the host down;
                // startup is where an unresolvable secret is fatal.
            }
        }
    }
}
