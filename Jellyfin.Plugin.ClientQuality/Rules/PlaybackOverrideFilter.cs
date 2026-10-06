using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ClientQuality.Rules;

/// <summary>
/// Global MVC action filter that rewrites the bound arguments of Jellyfin's playback-decision endpoints
/// for requests matching a configured rule, before the controller runs.
/// </summary>
/// <remarks>
/// Jellyfin.Api is not published as a package, so endpoints are matched by controller, action and
/// parameter name (Jellyfin 12.2). The controllers merge query arguments over the request body with
/// <c>??=</c>, so a value written into the query argument wins over whatever the client posted.
/// </remarks>
public sealed class PlaybackOverrideFilter : IAsyncActionFilter
{
    private const string MaxStreamingBitrateArg = "maxStreamingBitrate";

    // Controller name -> action names whose arguments decide direct play vs transcode.
    private static readonly Dictionary<string, string[]> _targets = new(StringComparer.Ordinal)
    {
        ["MediaInfo"] = ["GetPostedPlaybackInfo", "OpenLiveStream"],
        ["UniversalAudio"] = ["GetUniversalAudioStream"],
    };

    private static readonly (string Name, bool Value)[] _forceTranscodeArgs =
    [
        ("enableDirectPlay", false),
        ("enableDirectStream", false),
        ("allowVideoStreamCopy", false),
        ("enableTranscoding", true),
    ];

    private readonly IAuthorizationContext _authorizationContext;
    private readonly ILogger<PlaybackOverrideFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackOverrideFilter"/> class.
    /// </summary>
    /// <param name="authorizationContext">Instance of the <see cref="IAuthorizationContext"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{PlaybackOverrideFilter}"/> interface.</param>
    public PlaybackOverrideFilter(IAuthorizationContext authorizationContext, ILogger<PlaybackOverrideFilter> logger)
    {
        _authorizationContext = authorizationContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var rules = Plugin.Instance?.Configuration.Rules;
        if (rules is { Length: > 0 }
            && context.ActionDescriptor is ControllerActionDescriptor action
            && _targets.TryGetValue(action.ControllerName, out var actions)
            && actions.Contains(action.ActionName, StringComparer.Ordinal))
        {
            var auth = await _authorizationContext.GetAuthorizationInfo(context.HttpContext).ConfigureAwait(false);
            var requester = new PlaybackRequester(auth.UserId, auth.Client, auth.DeviceId);
            var playbackOverride = RuleResolver.Resolve(rules, requester);
            if (playbackOverride is not null)
            {
                Apply(context, action, playbackOverride);
                _logger.LogInformation(
                    "Client Quality: {Action} for user {UserId}, client {Client}, device {DeviceId}: force transcode {Force}, max bitrate {MaxBitrate}",
                    action.ActionName,
                    requester.UserId,
                    requester.Client,
                    requester.DeviceId,
                    playbackOverride.ForceTranscode,
                    context.ActionArguments.TryGetValue(MaxStreamingBitrateArg, out var maxBitrate) ? maxBitrate : null);
            }
        }

        await next().ConfigureAwait(false);
    }

    private static void Apply(ActionExecutingContext context, ControllerActionDescriptor action, PlaybackOverride playbackOverride)
    {
        var args = context.ActionArguments;

        // Unset query parameters are absent from ActionArguments rather than null, so add keys for any
        // parameter the action declares instead of only overwriting existing ones.
        bool HasParameter(string name) => action.Parameters.Any(p => string.Equals(p.Name, name, StringComparison.Ordinal));

        if (playbackOverride.ForceTranscode)
        {
            foreach (var (name, value) in _forceTranscodeArgs)
            {
                if (HasParameter(name))
                {
                    args[name] = value;
                }
            }
        }

        if (playbackOverride.MaxStreamingBitrate is int cap && HasParameter(MaxStreamingBitrateArg))
        {
            var requested = args.TryGetValue(MaxStreamingBitrateArg, out var queryValue) && queryValue is int q
                ? q
                : ReadBodyBitrate(action, args);
            args[MaxStreamingBitrateArg] = RuleResolver.ApplyCap(requested, cap);
        }
    }

    // The request DTO types live in Jellyfin.Api, so read MaxStreamingBitrate by name.
    private static int? ReadBodyBitrate(ControllerActionDescriptor action, IDictionary<string, object?> args)
    {
        var bodyParameter = action.Parameters.FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);
        if (bodyParameter is null || !args.TryGetValue(bodyParameter.Name, out var body) || body is null)
        {
            return null;
        }

        return body.GetType().GetProperty("MaxStreamingBitrate")?.GetValue(body) as int?;
    }
}
