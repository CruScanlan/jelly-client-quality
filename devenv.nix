{ pkgs, ... }:

{
  # .NET 10 SDK for building the Jellyfin 12.2 plugin.
  languages.dotnet = {
    enable = true;
    package = pkgs.dotnet-sdk_10;
  };

  env.DOTNET_CLI_TELEMETRY_OPTOUT = "1";

  scripts.build.exec = "dotnet publish Jellyfin.Plugin.ClientQuality -c Release -o artifacts";

  enterShell = ''
    echo "dotnet $(dotnet --version)"
  '';
}
