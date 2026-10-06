# Verify the PlaybackInfo filter on a running Jellyfin 12.2 server

Status: ready-for-human

The plugin's core mechanism is inferred from source and has not run against a real server.

## Check

1. Install the built DLL into `<data>/plugins/ClientQuality_1.0.0.0/`, restart, confirm the plugin shows as Active (not NotSupported/Malfunctioned).
2. Open Dashboard → Client Quality; confirm users and devices populate.
3. Add a rule: one device, Force transcode. Play a file that normally direct-plays on that device.
   - Expect: Dashboard → Activity / session shows "Transcoding"; server log has a `Client Quality:` line.
4. Add a rule: one user, cap 4 Mbps. Play a high-bitrate file as that user.
   - Expect: transcode with `TranscodeReasons` including `ContainerBitrateExceedsLimit`, and `VideoBitrate` ≤ 4 Mbps in the transcoding URL.
5. Check a client that posts only a JSON body (web) and one that uses query parameters, if available.

## If the filter doesn't fire

Fallbacks are listed in `docs/jellyfin-plugins/README.md` §10.3 (result filter on `PlaybackInfoResponse`, or middleware rewriting the request).

## Comments
