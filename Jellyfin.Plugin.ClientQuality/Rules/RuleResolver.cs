using System;
using System.Collections.Generic;
using Jellyfin.Plugin.ClientQuality.Configuration;

namespace Jellyfin.Plugin.ClientQuality.Rules;

/// <summary>
/// Who is asking to play something.
/// </summary>
/// <param name="UserId">The authenticated user id, or <see cref="Guid.Empty"/>.</param>
/// <param name="Client">The client app name from the authorization header.</param>
/// <param name="DeviceId">The device id from the authorization header.</param>
public sealed record PlaybackRequester(Guid UserId, string? Client, string? DeviceId);

/// <summary>
/// The combined effect of every rule matching a request.
/// </summary>
/// <param name="ForceTranscode">Whether any matching rule forces transcoding.</param>
/// <param name="MaxStreamingBitrate">The lowest bitrate cap among matching rules, if any.</param>
public sealed record PlaybackOverride(bool ForceTranscode, int? MaxStreamingBitrate);

/// <summary>
/// Resolves configured rules against a requester.
/// </summary>
public static class RuleResolver
{
    /// <summary>
    /// Combines all enabled rules matching <paramref name="requester"/>: transcoding is forced if any
    /// rule forces it, and the bitrate cap is the lowest positive cap.
    /// </summary>
    /// <param name="rules">The configured rules.</param>
    /// <param name="requester">The requester.</param>
    /// <returns>The override to apply, or <c>null</c> when no rule changes anything.</returns>
    public static PlaybackOverride? Resolve(IEnumerable<TranscodeRule> rules, PlaybackRequester requester)
    {
        var force = false;
        int? cap = null;

        foreach (var rule in rules)
        {
            if (!Matches(rule, requester))
            {
                continue;
            }

            force |= rule.ForceTranscode;
            if (rule.MaxStreamingBitrate > 0)
            {
                cap = cap is null ? rule.MaxStreamingBitrate : Math.Min(cap.Value, rule.MaxStreamingBitrate);
            }
        }

        return force || cap is not null ? new PlaybackOverride(force, cap) : null;
    }

    /// <summary>
    /// Whether a single rule matches a requester. Every non-empty criterion must match.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <param name="requester">The requester.</param>
    /// <returns><c>true</c> if the rule is enabled, has at least one criterion, and all its criteria match.</returns>
    public static bool Matches(TranscodeRule rule, PlaybackRequester requester)
    {
        if (!rule.Enabled)
        {
            return false;
        }

        var hasUser = !string.IsNullOrWhiteSpace(rule.UserId);
        var hasClient = !string.IsNullOrWhiteSpace(rule.Client);
        var hasDevice = !string.IsNullOrWhiteSpace(rule.DeviceId);

        if (!hasUser && !hasClient && !hasDevice)
        {
            return false;
        }

        // Guid.Empty means "no user" on the requester side, so it never counts as a user match.
        if (hasUser && !(Guid.TryParse(rule.UserId, out var userId) && userId != Guid.Empty && userId == requester.UserId))
        {
            return false;
        }

        if (hasClient && !string.Equals(rule.Client.Trim(), requester.Client?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (hasDevice && !string.Equals(rule.DeviceId.Trim(), requester.DeviceId?.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Applies a bitrate cap to the client's requested bitrate.
    /// </summary>
    /// <param name="requested">The client's requested bitrate, if any.</param>
    /// <param name="cap">The cap.</param>
    /// <returns>The lower of the two, or the cap when the client asked for nothing.</returns>
    public static int ApplyCap(int? requested, int cap)
        => requested is int r && r > 0 ? Math.Min(r, cap) : cap;
}
