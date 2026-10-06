using Jellyfin.Plugin.ClientQuality.Rules;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ClientQuality;

/// <summary>
/// Registers the plugin's services with the server.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Plugin services land in the same collection as the ASP.NET host, so this adds a global MVC filter.
        serviceCollection.Configure<MvcOptions>(options => options.Filters.Add<PlaybackOverrideFilter>());
    }
}
