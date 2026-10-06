// Source: https://github.com/jellyfin/jellyfin-plugin-transcodekiller/blob/3a5a5034e05892e0c2b1a289f7a69ae761f7a9d8/Jellyfin.Plugin.TranscodeKiller/Configuration/PluginConfiguration.cs
// Upstream: jellyfin/jellyfin-plugin-transcodekiller @ 3a5a5034e05892e0c2b1a289f7a69ae761f7a9d8
// Licence: GPL-3.0 (upstream LICENSE is the GPLv3 text). Copyright (c) the Jellyfin Contributors.
// Copied verbatim (BOM stripped) for reference only; not compiled in this repo.

using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TranscodeKiller.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the max width allowed to transcode.
    /// </summary>
    public int MaxWidth { get; set; } = 1920;

    /// <summary>
    /// Gets or sets the max height allowed to transcode.
    /// </summary>
    public int MaxHeight { get; set; } = 1080;
}
