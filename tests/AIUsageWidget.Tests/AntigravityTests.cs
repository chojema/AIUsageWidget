using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

public class AntigravityUsageParserTests
{
    // Same shape as `agy -p /usage --output-format json`, with fictional values.
    private const string Output = """
        {"conversation_id":"","status":"SUCCESS","response":"Gemini Models\tWeekly\t70%","duration_seconds":0.4,"num_turns":0,"usage":{"input_tokens":0,"output_tokens":0,"thinking_tokens":0,"cache_read_tokens":0,"total_tokens":0},"command":{"name":"usage","data":{"description":"Within each group, models share a weekly limit and a 5-hour limit.","groups":[{"name":"Gemini Models","description":"Models within this group: Gemini Flash, Gemini Pro","buckets":[{"id":"gemini-weekly","name":"Weekly Limit Remaining","window":"weekly","remaining_fraction":0.7,"reset_time":"2026-10-23T00:00:00Z"},{"id":"gemini-5h","name":"Five Hour Limit Remaining","window":"5h","remaining_fraction":0.25,"reset_time":"2026-10-18T12:30:00Z"}]},{"name":"Claude and GPT models","description":"Models within this group: example","buckets":[{"id":"3p-weekly","name":"Weekly Limit Remaining","window":"weekly","remaining_fraction":1,"reset_time":"2026-10-24T00:00:00Z"},{"id":"3p-5h","name":"Five Hour Limit Remaining","window":"5h","remaining_fraction":0.5,"reset_time":"2026-10-18T13:00:00Z"}]},{"name":"Gemini 4 Argon","description":"Weekly only","buckets":[{"id":"gemini-argon-wtu-weekly","name":"Gemini 4 Argon Weekly Limit","window":"weekly","remaining_fraction":0.9,"reset_time":"2026-10-25T00:00:00Z","description":"x"}]}]}}}
        """;

    [Fact]
    public void Reads_every_group_with_its_windows()
    {
        var (status, groups) = AntigravityUsageParser.Parse(Output);

        Assert.Equal(AntigravityParseStatus.Ok, status);
        Assert.Equal(["Gemini Models", "Claude and GPT models", "Gemini 4 Argon"], groups.Select(g => g.Name));

        AntigravityGroup gemini = groups[0];
        Assert.Equal(25, gemini.FiveHour!.RemainingPercent, 3);
        Assert.Equal(new DateTimeOffset(2026, 10, 18, 12, 30, 0, TimeSpan.Zero), gemini.FiveHour.ResetsAt);
        Assert.Equal(70, gemini.Weekly!.RemainingPercent, 3);

        Assert.Null(groups[2].FiveHour);
        Assert.Equal(90, groups[2].Weekly!.RemainingPercent, 3);
    }

    [Fact]
    public void Output_from_an_agent_turn_is_rejected()
    {
        // What happens when "/usage" reaches the model as a prompt: a turn runs and no command block exists.
        const string turn = """
            {"conversation_id":"abc","status":"SUCCESS","response":"text","num_turns":1,"usage":{"total_tokens":1000}}
            """;

        Assert.Equal(AntigravityParseStatus.Unsafe, AntigravityUsageParser.Parse(turn).Status);
    }

    [Fact]
    public void A_turn_count_above_zero_is_rejected_even_with_a_command_block()
    {
        string tampered = Output.Replace("\"num_turns\":0", "\"num_turns\":1");

        Assert.Equal(AntigravityParseStatus.Unsafe, AntigravityUsageParser.Parse(tampered).Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("""{"other":true}""")]
    public void Unrecognized_output_is_malformed(string output)
    {
        Assert.Equal(AntigravityParseStatus.Malformed, AntigravityUsageParser.Parse(output).Status);
    }

    [Fact]
    public void Reports_a_failed_command()
    {
        string failed = Output.Replace("\"status\":\"SUCCESS\"", "\"status\":\"ERROR\"");

        Assert.Equal(AntigravityParseStatus.Failed, AntigravityUsageParser.Parse(failed).Status);
    }

    [Fact]
    public void Ignores_log_lines_around_the_result()
    {
        var (status, groups) = AntigravityUsageParser.Parse("starting\n" + Output.Trim() + "\ndone\n");

        Assert.Equal(AntigravityParseStatus.Ok, status);
        Assert.Equal(3, groups.Count);
    }

    [Fact]
    public void Selection_falls_back_to_the_default_then_the_first_group()
    {
        var groups = AntigravityUsageParser.Parse(Output).Groups;

        Assert.Equal("Claude and GPT models", AntigravityUsageParser.Select(groups, "Claude and GPT models")!.Name);
        Assert.Equal("Gemini Models", AntigravityUsageParser.Select(groups, null)!.Name);
        Assert.Equal("Gemini Models", AntigravityUsageParser.Select(groups, "Removed group")!.Name);
        Assert.Equal("Gemini 4 Argon", AntigravityUsageParser.Select(groups.Skip(2).ToList(), "Gemini Models")!.Name);
        Assert.Null(AntigravityUsageParser.Select([], null));
    }

    [Theory]
    [InlineData("Gemini Models", "Gemini")]
    [InlineData("Claude and GPT models", "Claude and GPT")]
    [InlineData("Gemini 4 Argon", "Gemini 4 Argon")]
    public void Short_names_drop_the_models_suffix(string name, string expected)
    {
        Assert.Equal(expected, AntigravityUsageParser.ShortName(name));
    }

    [Fact]
    public void Posix_quoting_keeps_slash_arguments_literal()
    {
        Assert.Equal("'/usage'", CommandLocator.PosixQuote("/usage"));
        Assert.Equal("'it'\\''s'", CommandLocator.PosixQuote("it's"));
    }

    [Fact]
    public void Path_setup_appends_quoted_directories_after_the_login_path()
    {
        Assert.Equal(
            "export PATH=\"$PATH\":'/Users/me/.local/bin':'/opt/homebrew/bin'; ",
            CommandLocator.BuildPathSetup(["/Users/me/.local/bin", "/opt/homebrew/bin"]));
        Assert.Equal(string.Empty, CommandLocator.BuildPathSetup([]));
        Assert.Equal(string.Empty, CommandLocator.BuildPathSetup(["/bad:dir"]));
    }

    [Fact]
    public void Common_install_directories_include_the_user_local_bin()
    {
        Assert.Contains(Path.Combine("/Users/me", ".local", "bin"), CommandLocator.CommonInstallDirectories("/Users/me"));
        Assert.DoesNotContain(CommandLocator.CommonInstallDirectories(""), dir => dir.StartsWith(".", StringComparison.Ordinal));
    }
}
