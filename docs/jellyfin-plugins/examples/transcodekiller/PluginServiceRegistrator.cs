// Source: https://github.com/jellyfin/jellyfin-plugin-transcodekiller/blob/3a5a5034e05892e0c2b1a289f7a69ae761f7a9d8/Jellyfin.Plugin.TranscodeKiller/PluginServiceRegistrator.cs
// Upstream: jellyfin/jellyfin-plugin-transcodekiller @ 3a5a5034e05892e0c2b1a289f7a69ae761f7a9d8
// Licence: GPL-3.0 (upstream LICENSE is the GPLv3 text). Copyright (c) the Jellyfin Contributors.
// Copied verbatim (BOM stripped) for reference only; not compiled in this repo.

using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TranscodeKiller;

/// <summary>
/// Register services.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHostedService<KillerEntrypoint>();
    }
}
