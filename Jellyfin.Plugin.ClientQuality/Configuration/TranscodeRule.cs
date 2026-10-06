namespace Jellyfin.Plugin.ClientQuality.Configuration;

/// <summary>
/// A rule matching playback requests by user, client app and/or device.
/// Empty criteria match anything; a rule with no criteria at all never matches.
/// </summary>
public class TranscodeRule
{
    /// <summary>
    /// Gets or sets a value indicating whether the rule is active.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the Jellyfin user id to match, or empty for any user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client app name to match (e.g. "Jellyfin Web", "Jellyfin Android TV"), or empty for any client.
    /// Compared case-insensitively against the <c>Client</c> field of the request's authorization header.
    /// </summary>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device id to match, or empty for any device.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether matching requests must be transcoded
    /// (direct play, direct stream and video stream copy are disabled).
    /// </summary>
    public bool ForceTranscode { get; set; }

    /// <summary>
    /// Gets or sets the maximum streaming bitrate in bits per second, or 0 for no cap.
    /// The client's own requested bitrate is kept when it is lower.
    /// </summary>
    public int MaxStreamingBitrate { get; set; }
}
