// ORIGINAL SKETCH for this repo - NOT copied from upstream. UNCOMPILED and UNTESTED (no dotnet SDK was available
// when this was written). Treat as a starting point; every claim it relies on is listed in README.md
// ("Hooks relevant to per-client quality / forced transcoding", hook A) with a verified/inferred flag.
//
// Idea: a global MVC action filter, registered from IPluginServiceRegistrator, that rewrites the *bound action
// arguments* of Jellyfin's PlaybackInfo endpoints before the controller runs, based on the calling device.
// Jellyfin.Api (where MediaInfoController / PlaybackInfoDto live) is NOT a NuGet package, so the filter matches
// by controller/action name and parameter name (strings), not by type.
//
// Upstream facts used (jellyfin v12.2, commit ca0f16eb7c195f720a1493ed469a27c4657db0c6):
//   - POST Items/{itemId}/PlaybackInfo -> MediaInfoController.GetPostedPlaybackInfo(...)  (Jellyfin.Api/Controllers/MediaInfoController.cs:116-135)
//   - query parameters win over the body: `maxStreamingBitrate ??= playbackInfoDto?.MaxStreamingBitrate` etc. (same file:153-162)
//   - IAuthorizationContext.GetAuthorizationInfo(HttpContext) -> DeviceId/Client/Device/User (MediaBrowser.Controller/Net/IAuthorizationContext.cs)
//
// Registration (in your IPluginServiceRegistrator.RegisterServices):
//   serviceCollection.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o => o.Filters.Add<PlaybackInfoQualityFilter>());

using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jellyfin.Plugin.ClientQuality;

/// <summary>Per-device override of PlaybackInfo arguments (sketch).</summary>
public sealed class PlaybackInfoQualityFilter : IAsyncActionFilter
{
    private readonly IAuthorizationContext _authContext;

    public PlaybackInfoQualityFilter(IAuthorizationContext authContext) => _authContext = authContext;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor d
            && d.ControllerName == "MediaInfo"
            && (d.ActionName == "GetPostedPlaybackInfo" || d.ActionName == "OpenLiveStream"))
        {
            var auth = await _authContext.GetAuthorizationInfo(context.HttpContext).ConfigureAwait(false);

            // Hypothetical plugin config lookup keyed by DeviceId (stable per install) with Client as fallback.
            var rule = ClientQualityPlugin.Instance?.Configuration.FindRule(auth.DeviceId, auth.Client);
            if (rule is not null)
            {
                var args = context.ActionArguments;

                if (rule.MaxStreamingBitrate is int max && args.ContainsKey("maxStreamingBitrate"))
                {
                    // Query value takes precedence over the body in the controller, so overwrite the query arg.
                    // (Capping with min(clientValue, max) would need reflection on the body DTO's MaxStreamingBitrate.)
                    args["maxStreamingBitrate"] = max;
                }

                if (rule.ForceTranscode)
                {
                    // Names match the controller's parameter names (MediaInfoController.cs:119-135).
                    SetIfPresent(args, "enableDirectPlay", false);
                    SetIfPresent(args, "enableDirectStream", false);
                    SetIfPresent(args, "allowVideoStreamCopy", false);
                    SetIfPresent(args, "allowAudioStreamCopy", false);
                }
            }
        }

        await next().ConfigureAwait(false);
    }

    private static void SetIfPresent(System.Collections.Generic.IDictionary<string, object?> args, string key, object value)
    {
        if (args.ContainsKey(key))
        {
            args[key] = value;
        }
    }
}
