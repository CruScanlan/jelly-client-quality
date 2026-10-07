using System.IO;
using Xunit;

namespace Jellyfin.Plugin.ClientQuality.Tests;

public class ConfigPageTests
{
    private const string ResourceName = "Jellyfin.Plugin.ClientQuality.Configuration.configPage.html";

    private static string ReadPage()
    {
        var stream = typeof(Plugin).Assembly.GetManifestResourceStream(ResourceName);
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public void ConfigPage_IsEmbeddedResource()
    {
        Assert.Contains(ResourceName, typeof(Plugin).Assembly.GetManifestResourceNames());
    }

    [Fact]
    public void ConfigPage_ContainsNoTemplateLiteralPlaceholders()
    {
        // The dashboard passes the whole page (including inline scripts) through
        // globalize.translateHtml, which rewrites every "${...}" into a translation
        // lookup and breaks JS template literals.
        Assert.DoesNotContain("${", ReadPage());
    }

    [Fact]
    public void ConfigPage_CreateElementCalls_DoNotPassOptionsObject()
    {
        // The Jellyfin 12.2 dashboard swaps document.createElement for the
        // webcomponents.js 0.7 (custom elements v0) polyfill, which calls
        // .toLowerCase() on the second argument. The v1 form
        // createElement('select', { is: 'emby-select' }) therefore throws; the
        // string form createElement('select', 'emby-select') must be used.
        Assert.DoesNotMatch(@"createElement\s*\([^)]*\{", ReadPage());
    }

    [Fact]
    public void ConfigPage_ReferencesPluginId()
    {
        Assert.Contains(Plugin.PluginId.ToString(), ReadPage());
    }
}
