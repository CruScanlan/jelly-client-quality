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
    public void ConfigPage_ReferencesPluginId()
    {
        Assert.Contains(Plugin.PluginId.ToString(), ReadPage());
    }
}
