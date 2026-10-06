# Client Quality (Jellyfin plugin)

Force transcoding, or cap streaming bitrate, for specific users, client apps or devices. Rules are managed from the Jellyfin dashboard.

Targets **Jellyfin 12.2** (`net10.0`, `Jellyfin.Controller` 12.2.0, `targetAbi` 12.2.0.0).

## What it does

In the dashboard (main menu → **Client Quality**, or Plugins → Client Quality), add rules. Each rule has:

| Field | Meaning |
|---|---|
| User | A Jellyfin user, or Any |
| Client app | A client app name as reported by the client, e.g. `Jellyfin Web`, `Jellyfin Android TV`, or Any |
| Device | A specific device the server has seen, or Any |
| Force transcode | Disable direct play, direct stream and video stream copy for matching requests, so the server re-encodes |
| Max streaming bitrate | Cap the bitrate for matching requests. The client's own setting is kept if it's lower |

All criteria that aren't "Any" must match (AND). A rule with every criterion set to Any is ignored. When several rules match, transcoding is forced if any rule forces it, and the lowest bitrate cap wins.

Examples: force transcode for everything played on one TV; force transcode for user "kids" on any client; cap `Jellyfin Web` to 8 Mbps for everyone.

## How it works

Jellyfin has no plugin API for changing playback decisions, so the plugin registers a global ASP.NET MVC action filter (`Rules/PlaybackOverrideFilter.cs`). Before these endpoints run, it rewrites their bound arguments for matching requests:

- `POST /Items/{id}/PlaybackInfo` (`MediaInfo.GetPostedPlaybackInfo`): `enableDirectPlay`, `enableDirectStream`, `allowVideoStreamCopy` → false, `enableTranscoding` → true, `maxStreamingBitrate` → capped
- `POST /LiveStreams/Open` (`MediaInfo.OpenLiveStream`): the same, where the parameter exists
- `GET /Audio/{id}/universal` (`UniversalAudio.GetUniversalAudioStream`): bitrate cap only

The controllers prefer query arguments over the posted body (`??=`), so the rewritten values win. The requester is identified with `IAuthorizationContext` (user, `Client`, `DeviceId` from the auth header). Background and source links: [docs/jellyfin-plugins/README.md](docs/jellyfin-plugins/README.md), section 10.

## Limitations

- **Untested on a running server.** The filter approach is inferred from Jellyfin 12.2 source; see `.scratch/client-quality/issues/01-verify-on-server.md`.
- Forcing a transcode needs the user's "Allow video playback that requires transcoding" permission. Without it the server can't transcode and playback fails.
- The web client's audio player builds `/Audio/{id}/universal` URLs itself, so web *audio* can be bitrate-capped but not forced to transcode.
- The client UI doesn't know about the override; its quality menu still shows its own setting.
- Device ids change if a browser's storage is cleared. Prefer user and client rules for browsers.
- Changes apply to the next playback started.
- Endpoints are matched by controller, action and parameter name, so a future Jellyfin release that renames them will silently stop the filter working. Recheck on each major upgrade.

## Build and install

```bash
dotnet publish Jellyfin.Plugin.ClientQuality -c Release -o artifacts
```

Copy `artifacts/Jellyfin.Plugin.ClientQuality.dll` into `<jellyfin data dir>/plugins/ClientQuality_1.0.0.0/` and restart Jellyfin. Packaging for a plugin repository uses `build.yaml` with [JPRM](https://github.com/oddstr13/jellyfin-plugin-repository-manager).

## Licence

GPL-3.0. See [LICENSE](LICENSE).
