using AIUsageWidget.Platform.Mac;
using Xunit;

namespace AIUsageWidget.Tests;

public class MacStartupTests
{
    private const string BundleExe = "/Applications/AIUsage Widget & Co.app/Contents/MacOS/AIUsageWidget";

    [Fact]
    public void FindsBundleOnlyForAppLayout()
    {
        Assert.Equal("/Applications/AIUsage Widget & Co.app", MacStartup.FindBundle(BundleExe));
        Assert.Null(MacStartup.FindBundle("/Users/me/src/bin/Debug/AIUsageWidget"));
        Assert.Null(MacStartup.FindBundle("/Applications/Foo.app/Contents/Resources/AIUsageWidget"));
    }

    [Fact]
    public void PlistOpensBundleWithEscapedPath()
    {
        string plist = MacStartup.BuildPlist(BundleExe);
        Assert.Contains("<string>/usr/bin/open</string>", plist);
        Assert.Contains("<string>/Applications/AIUsage Widget &amp; Co.app</string>", plist);
        Assert.Contains("<key>RunAtLoad</key><true/>", plist);
        Assert.Contains($"<string>{MacStartup.Label}</string>", plist);
        Assert.StartsWith("<?xml", plist);
    }

    [Fact]
    public void PlistRunsExecutableDirectlyOutsideBundle()
    {
        string plist = MacStartup.BuildPlist("/tmp/out/AIUsageWidget");
        Assert.DoesNotContain("/usr/bin/open", plist);
        Assert.Contains("<string>/tmp/out/AIUsageWidget</string>", plist);
    }

    [Fact]
    public void EnableAndDisableRoundTrip()
    {
        string dir = Path.Combine(Path.GetTempPath(), "aiuw-startup-" + Guid.NewGuid().ToString("N"));
        string agent = Path.Combine(dir, "LaunchAgents", MacStartup.Label + ".plist");
        try
        {
            Assert.False(MacStartup.IsEnabled(agent, BundleExe));
            MacStartup.SetEnabled(agent, BundleExe, true);
            Assert.True(MacStartup.IsEnabled(agent, BundleExe));
            // A moved app no longer matches the saved agent.
            Assert.False(MacStartup.IsEnabled(agent, "/Users/me/Apps/AIUsageWidget.app/Contents/MacOS/AIUsageWidget"));
            MacStartup.SetEnabled(agent, BundleExe, false);
            Assert.False(File.Exists(agent));
            MacStartup.SetEnabled(agent, BundleExe, false); // Turning off twice is harmless.
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
