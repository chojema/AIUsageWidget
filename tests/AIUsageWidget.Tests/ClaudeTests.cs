using System.Text.Json.Nodes;
using AIUsageWidget.Claude;
using AIUsageWidget.Core;
using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

public class ClaudeUsageRecordTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1_738_400_000);

    [Fact]
    public void Reads_rate_limits_from_status_line_input()
    {
        const string input = """
            {
              "model": { "display_name": "Example" },
              "workspace": { "current_dir": "/home/example/project" },
              "rate_limits": {
                "five_hour": { "used_percentage": 23.5, "resets_at": 1738425600 },
                "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 }
              }
            }
            """;

        ClaudeUsageRecord record = ClaudeUsageRecord.FromStatusLineInput(input, Now)!;

        Assert.Equal(76.5, record.FiveHour!.RemainingPercent, 3);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1738857600), record.Weekly!.ResetsAt);
    }

    [Fact]
    public void Cache_contains_only_limit_windows()
    {
        const string input = """
            { "workspace": { "current_dir": "/home/example/secret-project" },
              "rate_limits": { "five_hour": { "used_percentage": 1, "resets_at": 1738425600 } } }
            """;

        string json = ClaudeUsageRecord.FromStatusLineInput(input, Now)!.ToJson();

        Assert.DoesNotContain("secret-project", json);
        Assert.Equal(1, ClaudeUsageRecord.FromJson(json)!.FiveHour!.UsedPercent);
        Assert.Null(ClaudeUsageRecord.FromJson(json)!.Weekly);
    }

    [Theory]
    [InlineData("""{ "model": {} }""")]
    [InlineData("""{ "rate_limits": {} }""")]
    [InlineData("not json")]
    public void Missing_limits_produce_no_record(string input)
    {
        Assert.Null(ClaudeUsageRecord.FromStatusLineInput(input, Now));
    }

    [Fact]
    public void Expired_windows_are_not_shown()
    {
        var record = new ClaudeUsageRecord(
            new UsageWindow(50, Now.AddMinutes(-1)),
            new UsageWindow(30, Now.AddDays(2)),
            Now.AddHours(-3));

        ProviderSnapshot snapshot = ClaudeProvider.ToSnapshot(record, Now);

        Assert.Equal(ProviderState.Ok, snapshot.State);
        Assert.Null(snapshot.FiveHour);
        Assert.Equal(30, snapshot.Weekly!.UsedPercent);
    }

    [Fact]
    public void No_record_means_waiting()
    {
        Assert.Equal(ProviderState.WaitingForData, ClaudeProvider.ToSnapshot(null, Now).State);
    }
}

public class ClaudeIntegrationTests
{
    private const string Bridge = "\"C:/Tools/AIUsageWidget.exe\" claude-statusline";

    [Fact]
    public void Install_keeps_other_settings_and_display_options()
    {
        var settings = JsonNode.Parse("""
            { "model": "example", "statusLine": { "type": "command", "command": "my-line.sh", "padding": 2 } }
            """)!.AsObject();

        var (updated, previous) = ClaudeIntegration.ApplyInstall(settings, Bridge);

        Assert.Equal("example", updated["model"]!.GetValue<string>());
        Assert.Equal(Bridge, updated["statusLine"]!["command"]!.GetValue<string>());
        Assert.Equal(2, updated["statusLine"]!["padding"]!.GetValue<int>());
        Assert.Equal("my-line.sh", previous!["command"]!.GetValue<string>());
    }

    [Fact]
    public void Reinstall_does_not_record_the_bridge_as_previous()
    {
        var settings = JsonNode.Parse($$"""{ "statusLine": { "type": "command", "command": {{JsonValue.Create(Bridge).ToJsonString()}} } }""")!.AsObject();

        var (_, previous) = ClaudeIntegration.ApplyInstall(settings, Bridge);

        Assert.Null(previous);
    }

    [Fact]
    public void Uninstall_restores_previous_status_line()
    {
        var original = JsonNode.Parse("""{ "type": "command", "command": "my-line.sh" }""")!.AsObject();
        var settings = JsonNode.Parse("""{ "theme": "dark" }""")!.AsObject();
        var (installed, previous) = ClaudeIntegration.ApplyInstall(settings, Bridge);
        Assert.Null(previous);

        JsonObject restored = ClaudeIntegration.ApplyUninstall(installed, original);

        Assert.Equal("my-line.sh", restored["statusLine"]!["command"]!.GetValue<string>());
        Assert.Equal("dark", restored["theme"]!.GetValue<string>());
    }

    [Fact]
    public void Uninstall_without_previous_removes_the_key()
    {
        var (installed, _) = ClaudeIntegration.ApplyInstall(new JsonObject(), Bridge);

        Assert.False(ClaudeIntegration.ApplyUninstall(installed, null).ContainsKey("statusLine"));
    }

    [Fact]
    public void Uninstall_leaves_a_status_line_changed_by_someone_else()
    {
        var settings = JsonNode.Parse("""{ "statusLine": { "type": "command", "command": "other.sh" } }""")!.AsObject();

        JsonObject result = ClaudeIntegration.ApplyUninstall(settings, null);

        Assert.Equal("other.sh", result["statusLine"]!["command"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(ClaudeShell.Kind.GitBash, @"C:\Program Files\AIUsageWidget\AIUsageWidget.exe",
        "\"C:/Program Files/AIUsageWidget/AIUsageWidget.exe\" claude-statusline")]
    [InlineData(ClaudeShell.Kind.PowerShell, @"C:\Apps\AIUsageWidget.exe",
        @"& 'C:\Apps\AIUsageWidget.exe' claude-statusline")]
    [InlineData(ClaudeShell.Kind.Posix, "/Applications/AIUsageWidget.app/Contents/MacOS/AIUsageWidget",
        "'/Applications/AIUsageWidget.app/Contents/MacOS/AIUsageWidget' claude-statusline")]
    public void Bridge_command_is_quoted_for_the_shell(ClaudeShell.Kind kind, string exe, string expected)
    {
        Assert.Equal(expected, ClaudeShell.QuoteInvocation(exe, ClaudeStatusLineBridge.Argument, kind));
        Assert.True(ClaudeIntegration.IsBridgeCommand(expected));
    }
}
