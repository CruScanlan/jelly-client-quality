#!/usr/bin/env python3
"""Add a plugin release to a Jellyfin plugin repository manifest.

The manifest is the JSON file Jellyfin reads when it is added under
Dashboard -> Plugins -> Repositories: an array of plugins, each with a
`versions` list (newest first). Plugin metadata comes from build.yaml,
converted to JSON first (`yq -o=json build.yaml`).
"""

import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path


def md5(path: Path) -> str:
    digest = hashlib.md5()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def version_key(version: str) -> tuple[int, ...]:
    return tuple(int(part) for part in version.split("."))


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, required=True, help="manifest.json to create or update")
    parser.add_argument("--build-json", type=Path, required=True, help="build.yaml converted to JSON")
    parser.add_argument("--zip", type=Path, required=True, help="the release zip")
    parser.add_argument("--version", required=True, help="four-part plugin version, e.g. 1.0.0.0")
    parser.add_argument("--source-url", required=True, help="public download URL of the zip")
    args = parser.parse_args()

    build = json.loads(args.build_json.read_text())
    plugins = json.loads(args.manifest.read_text()) if args.manifest.exists() else []

    existing = next((p for p in plugins if p["guid"] == build["guid"]), None)
    # Refresh the listing from build.yaml so description changes show up in Jellyfin.
    plugin = {
        "guid": build["guid"],
        "name": build["name"],
        "description": build["description"].strip(),
        "overview": build["overview"],
        "owner": build["owner"],
        "category": build["category"],
        "versions": existing["versions"] if existing else [],
    }
    if existing:
        plugins[plugins.index(existing)] = plugin
    else:
        plugins.append(plugin)

    entry = {
        "version": args.version,
        "changelog": build.get("changelog", "").strip(),
        "targetAbi": build["targetAbi"],
        "sourceUrl": args.source_url,
        "checksum": md5(args.zip),
        "timestamp": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
    }

    # Re-running a release replaces its entry instead of duplicating it.
    versions = [v for v in plugin["versions"] if v["version"] != args.version]
    versions.append(entry)
    versions.sort(key=lambda v: version_key(v["version"]), reverse=True)
    plugin["versions"] = versions

    args.manifest.write_text(json.dumps(plugins, indent=2) + "\n")
    print(f"{build['name']} {args.version}: checksum {entry['checksum']}")


if __name__ == "__main__":
    main()
