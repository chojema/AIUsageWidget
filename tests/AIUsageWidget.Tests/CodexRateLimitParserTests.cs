using System.Text.Json.Nodes;
using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

public class CodexRateLimitParserTests
{
    [Fact]
    public void Maps_windows_by_duration_not_by_position()
    {
        // Secondary is the 5-hour window here; the parser must not assume primary = 5 hours.
        JsonNode snapshot = JsonNode.Parse("""
            {
              "primary":   { "usedPercent": 40, "windowDurationMins": 10080, "resetsAt": 1800000000 },
              "secondary": { "usedPercent": 12, "windowDurationMins": 300,   "resetsAt": 1790000000 }
            }
            """)!;

        var (fiveHour, weekly) = CodexRateLimitParser.Parse(snapshot);

        Assert.Equal(12, fiveHour!.UsedPercent);
        Assert.Equal(88, fiveHour.RemainingPercent);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1790000000), fiveHour.ResetsAt);
        Assert.Equal(40, weekly!.UsedPercent);
    }

    [Fact]
    public void Ignores_windows_with_unknown_duration()
    {
        JsonNode snapshot = JsonNode.Parse("""
            { "primary": { "usedPercent": 5 }, "secondary": { "usedPercent": 9, "windowDurationMins": 60 } }
            """)!;

        var (fiveHour, weekly) = CodexRateLimitParser.Parse(snapshot);

        Assert.Null(fiveHour);
        Assert.Null(weekly);
    }

    [Fact]
    public void Sparse_update_keeps_previous_values()
    {
        var current = CodexRateLimitParser.Parse(JsonNode.Parse("""
            { "primary": { "usedPercent": 10, "windowDurationMins": 300 },
              "secondary": { "usedPercent": 20, "windowDurationMins": 10080 } }
            """));

        var merged = CodexRateLimitParser.Merge(current, JsonNode.Parse("""
            { "primary": { "usedPercent": 15, "windowDurationMins": 300 }, "secondary": null }
            """));

        Assert.Equal(15, merged.FiveHour!.UsedPercent);
        Assert.Equal(20, merged.Weekly!.UsedPercent);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("50.6504940000", "50.6504940000")]
    public void Reads_credit_balance_without_converting_it_to_currency(string balance, string expected)
    {
        var snapshot = new JsonObject { ["credits"] = new JsonObject
        {
            ["hasCredits"] = balance != "0", ["unlimited"] = false, ["balance"] = balance,
        } };

        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            CodexRateLimitParser.ParseCredits(snapshot)!.Balance);
    }

    [Fact]
    public void Unlimited_credits_do_not_require_a_numeric_balance()
    {
        var credits = CodexRateLimitParser.ParseCredits(JsonNode.Parse(
            """{"credits":{"hasCredits":true,"unlimited":true,"balance":null}}"""));

        Assert.True(credits!.Unlimited);
        Assert.Null(credits.Balance);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"credits":null}""")]
    [InlineData("""{"credits":{"balance":null}}""")]
    [InlineData("""{"credits":{"balance":"invalid"}}""")]
    [InlineData("""{"credits":{"balance":"-1"}}""")]
    [InlineData("""{"credits":{"balance":false}}""")]
    public void Unknown_or_invalid_credits_are_not_reported_as_zero(string json)
    {
        Assert.Null(CodexRateLimitParser.ParseCredits(JsonNode.Parse(json)));
    }

    [Fact]
    public void Credit_updates_keep_missing_values_but_clear_an_explicitly_unavailable_balance()
    {
        using var provider = new CodexProvider();
        provider.HandleReadResponse((JsonObject)JsonNode.Parse("""
            {"result":{"rateLimits":{"primary":{"usedPercent":10,"windowDurationMins":300},
              "credits":{"hasCredits":true,"unlimited":false,"balance":"50.65"}}}}
            """)!);
        Assert.Equal(50.65m, provider.Current.Credits!.Balance);
        provider.HandleNotification((JsonObject)JsonNode.Parse("""
            {"method":"account/rateLimits/updated","params":{"rateLimits":{
              "primary":{"usedPercent":20,"windowDurationMins":300}}}}
            """)!);
        Assert.Equal(50.65m, provider.Current.Credits!.Balance);
        Assert.Equal(20, provider.Current.FiveHour!.UsedPercent);
        provider.HandleNotification((JsonObject)JsonNode.Parse("""
            {"method":"account/rateLimits/updated","params":{"rateLimits":{"credits":null}}}
            """)!);
        Assert.Null(provider.Current.Credits);
        Assert.Equal(20, provider.Current.FiveHour!.UsedPercent);
    }

    [Fact]
    public void Credit_only_accounts_can_still_publish_a_known_zero_balance()
    {
        using var provider = new CodexProvider();
        provider.HandleReadResponse((JsonObject)JsonNode.Parse("""
            {"result":{"rateLimits":{"credits":{"hasCredits":false,"unlimited":false,"balance":"0"}}}}
            """)!);
        Assert.Equal(AIUsageWidget.Core.ProviderState.Ok, provider.Current.State);
        Assert.Equal(0m, provider.Current.Credits!.Balance);
    }

    [Fact]
    public void Null_snapshot_yields_nothing()
    {
        Assert.Equal((null, null), CodexRateLimitParser.Parse(null));
    }
}
