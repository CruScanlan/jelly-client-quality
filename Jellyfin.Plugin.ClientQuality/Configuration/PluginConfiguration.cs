using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ClientQuality.Configuration;

/// <summary>
/// Plugin configuration, edited from the dashboard config page.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the playback rules. Every enabled rule that matches a request applies.
    /// </summary>
    public TranscodeRule[] Rules { get; set; } = [];
}
