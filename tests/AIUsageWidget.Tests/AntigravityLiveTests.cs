using System.Diagnostics;
using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

/// <summary>
/// Runs the real CLI. Opt in with AIUSAGEWIDGET_LIVE_AGY=1 on a machine where agy is installed and signed in.
/// Only the structure is checked; no usage values are printed.
/// </summary>
public class AntigravityLiveTests
{
    [Fact]
    public async Task Real_cli_answers_usage_without_an_agent_turn()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable("AIUSAGEWIDGET_LIVE_AGY") == "1",
            "Set AIUSAGEWIDGET_LIVE_AGY=1 to run against the installed Antigravity CLI.");

        ProcessStartInfo start = CommandLocator.Build("agy", "-p", "/usage", "--output-format", "json")
                                 ?? throw new InvalidOperationException("agy not found");
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        using Process process = Process.Start(start)!;
        process.StandardInput.Close();
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(90));

        var (status, groups) = AntigravityUsageParser.Parse(await output);

        Assert.Equal(AntigravityParseStatus.Ok, status);
        Assert.NotEmpty(groups);
        Assert.Contains(groups, g => g.FiveHour is not null || g.Weekly is not null);
        Assert.NotNull(AntigravityUsageParser.Select(groups, null));
    }
}
