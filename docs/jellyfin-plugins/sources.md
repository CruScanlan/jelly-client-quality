# Sources

Accessed 2026-10-06. "Pinned" = commit/tag all line references and copied files refer to. Licences read from each repo's `LICENSE` file (and GitHub's detection where noted).

## Documentation (paraphrased / linked only, nothing copied)

| Source | URL | Pinned | Licence | Used for |
|---|---|---|---|---|
| jellyfin.org docs: Plugins | https://jellyfin.org/docs/general/server/plugins/ | source: jellyfin-docs `bd15bca…` (`general/server/plugins/index.md`); also jellyfin.org repo `819d645…` (`docs/general/server/plugins/index.mdx`) | jellyfin-docs: CC BY-SA 4.0; jellyfin.org repo: CC BY-ND 4.0 (no derivatives, so paraphrase/link only) | install paths, catalog, official repo URLs |
| Blog "Plugin Repositories" (2020-07-17) | https://jellyfin.org/posts/plugin-updates/ | jellyfin.org `819d645c2a7c8e53d7f6e83e6f3761fbd9275a82`, `blog/2020/07-17-plugin-updates.mdx` | CC BY-ND 4.0 | manifest format |
| Live official manifest | https://repo.jellyfin.org/files/plugin/manifest.json | fetched 2026-10-06 (≈200 KB) | data | current manifest shape, targetAbi per plugin |
| jellyfin-docs (plugin-api docfx stub, contributing pages) | https://github.com/jellyfin/jellyfin-docs | `bd15bcac098f6b22517bae83a2e6e891f2a54f7d` | CC BY-SA 4.0 | confirms no plugin-dev guide exists |
| REST API reference | https://api.jellyfin.org | n/a (not fetched; generated from server source) | n/a | pointer only |
| Microsoft docs (ASP.NET filters/IStartupFilter, hosted services) | https://learn.microsoft.com/aspnet/core/ | not fetched; referenced only through the template README link and from general knowledge — hook A/C feasibility is flagged `[I]` for this reason | n/a | — |

## Template and official plugins

| Repo | URL | Pinned commit | Licence | Copied into `examples/`? |
|---|---|---|---|---|
| jellyfin-plugin-template | https://github.com/jellyfin/jellyfin-plugin-template | `c93225a0a5a76d3843db05b4b5b77fcfc482fba3` (2026-09-17) | GPL-3.0 (LICENSE = GPLv3 text; README requires GPLv3 or compatible permissive) | Yes — `examples/template/` |
| jellyfin-plugin-transcodekiller | https://github.com/jellyfin/jellyfin-plugin-transcodekiller | `3a5a5034e05892e0c2b1a289f7a69ae761f7a9d8` | GPL-3.0 | Yes — `examples/transcodekiller/` |
| jellyfin-plugin-sessioncleaner | https://github.com/jellyfin/jellyfin-plugin-sessioncleaner | `cec3427ceae864a8d57fd25be90a0f9af62b7915` | GPL-3.0 | Yes — `SessionCleanerTask.cs` |
| jellyfin-plugin-webhook | https://github.com/jellyfin/jellyfin-plugin-webhook | `ede062d6106092b209d31adcf6d64b81a4be7013` | GPL-3.0 | Yes — `PlaybackStartNotifier.cs`, `PluginServiceRegistrator.cs` |
| jellyfin-plugin-opensubtitles | https://github.com/jellyfin/jellyfin-plugin-opensubtitles | `60d2c74d4e8bbee738fa88288fa5c583036c4183` | GPL-3.0 | Yes — `OpenSubtitlesController.cs` |
| jellyfin-plugin-playbackreporting | https://github.com/jellyfin/jellyfin-plugin-playbackreporting | `814e507d06bf2f8186fabc182401333ae24d5394` | GPL-3.0 (files also carry a "Copyright(C) 2018 … GPL v3" header) | No — linked only (controller w/ RequiresElevation) |
| jellyfin-plugin-tvdb, -kodisyncqueue, -chapter-segments | https://github.com/jellyfin | tvdb `f1bcc9e`, kodisyncqueue `a1cdd8d`, chapter-segments `3b6a877` | GPL-3.0 | No — skimmed for scheduled-task / controller / media-segment patterns |
| jellyfin-plugin-reports | https://github.com/jellyfin/jellyfin-plugin-reports | `9a0b777` | **MIT** | No — not used |
| jellyfin-meta-plugins | https://github.com/jellyfin/jellyfin-meta-plugins | `5ebf9f9455736773ab6f94cdb19fb903ece41b83` | no LICENSE file at repo root (GitHub: none); `build_plugin.sh` carries a per-file Unlicense header | No — linked (reusable CI workflows `build.yaml`/`publish.yaml`) |
| JPRM (jellyfin-plugin-repository-manager) | https://github.com/oddstr13/jellyfin-plugin-repository-manager | README read via API | MPL-2.0 | No — tool, linked |

## Server and clients (read for hook points; linked only — **not copied**)

| Repo | URL | Pinned | Licence | Notes |
|---|---|---|---|---|
| jellyfin/jellyfin | https://github.com/jellyfin/jellyfin | tag **v12.2** = `ca0f16eb7c195f720a1493ed469a27c4657db0c6` (2026-10-05) | Repo `LICENSE`: **GPL-2.0**. Published NuGet packages (Jellyfin.Controller/Common/Model/Data/Extensions…): `PackageLicenseExpression` **GPL-3.0-only** (e.g. `MediaBrowser.Controller.csproj:13`) | Because the repo licence (GPL-2.0) and the package licence (GPL-3.0-only) differ and this repo's licence is undecided, no server source was copied; README cites file:line permalinks only. |
| jellyfin/jellyfin-web | https://github.com/jellyfin/jellyfin-web | tag **v12.2** = `61b1f890365ad15138e943e694af723c1d47df2a` | GPL-2.0 | per-device quality settings |
| jellyfin/jellyfin-androidtv | https://github.com/jellyfin/jellyfin-androidtv | `398bfffcb075dab81300cb1ab03afb331f93f0a0` (master, 2026-10-06) | GPL-2.0 | `pref_max_bitrate` |
| jellyfin/jellyfin-android | https://github.com/jellyfin/jellyfin-android | `d9a0768d3ffbc2658ed6ac1fa2beae3f52075bd5` (master) | GPL-2.0 | reads web player bitrate prefs |
| jellyfin-roku, jellyfin-vue | https://github.com/jellyfin | roku `48621d7`, vue `d198435` | GPL-2.0 / GPL-3.0 | grepped for bitrate terms only; not analysed |
| NuGet | https://www.nuget.org/packages/Jellyfin.Controller | versions list fetched via `api.nuget.org` 2026-10-06 (latest stable 12.2.0) | GPL-3.0-only | package versions |

## Original material in this folder

`README.md`, `sources.md`, `examples/README.md` and `examples/sketches/PlaybackInfoQualityFilter.cs` are original to this repo (no upstream licence attached; licence follows the repo's eventual choice). All other files under `examples/` are verbatim upstream copies (GPL-3.0) with a source/licence header comment prepended (UTF-8 BOMs stripped).
