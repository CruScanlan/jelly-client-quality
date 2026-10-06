using System;
using Jellyfin.Plugin.ClientQuality.Configuration;
using Jellyfin.Plugin.ClientQuality.Rules;
using Xunit;

namespace Jellyfin.Plugin.ClientQuality.Tests;

public class RuleResolverTests
{
    private static readonly Guid _user = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid _otherUser = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private static PlaybackRequester Requester(Guid? userId = null, string? client = "Jellyfin Web", string? deviceId = "device-1")
        => new(userId ?? _user, client, deviceId);

    private static TranscodeRule Rule(
        string userId = "",
        string client = "",
        string deviceId = "",
        bool enabled = true,
        bool force = false,
        int maxBitrate = 0)
        => new()
        {
            Enabled = enabled,
            UserId = userId,
            Client = client,
            DeviceId = deviceId,
            ForceTranscode = force,
            MaxStreamingBitrate = maxBitrate,
        };

    // ---- Matches: gating ----

    [Fact]
    public void Matches_DisabledRule_NeverMatches()
    {
        var rule = Rule(userId: _user.ToString(), client: "Jellyfin Web", deviceId: "device-1", enabled: false);

        Assert.False(RuleResolver.Matches(rule, Requester()));
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("   ", "\t", " \n ")]
    public void Matches_NoCriteria_NeverMatches(string userId, string client, string deviceId)
    {
        var rule = Rule(userId, client, deviceId);

        Assert.False(RuleResolver.Matches(rule, Requester()));
    }

    // ---- Matches: user ----

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    public void Matches_UserCriterion_AcceptsDashedAndUndashedGuid(string format)
    {
        var rule = Rule(userId: _user.ToString(format));

        Assert.True(RuleResolver.Matches(rule, Requester()));
    }

    [Fact]
    public void Matches_UserCriterion_IsCaseInsensitiveHexAndTrimmed()
    {
        var rule = Rule(userId: "  " + _otherUser.ToString().ToUpperInvariant() + "  ");

        Assert.True(RuleResolver.Matches(rule, Requester(userId: _otherUser)));
    }

    [Fact]
    public void Matches_UserCriterion_DifferentUser_DoesNotMatch()
    {
        var rule = Rule(userId: _otherUser.ToString());

        Assert.False(RuleResolver.Matches(rule, Requester()));
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("1234")]
    public void Matches_UserCriterion_UnparseableGuid_DoesNotMatch(string userId)
    {
        var rule = Rule(userId: userId);

        Assert.False(RuleResolver.Matches(rule, Requester()));
    }

    [Fact]
    public void Matches_UserCriterion_UnparseableGuid_DoesNotMatchAnonymousRequester()
    {
        var rule = Rule(userId: "garbage");

        Assert.False(RuleResolver.Matches(rule, Requester(userId: Guid.Empty)));
    }

    // ---- Matches: client ----

    [Theory]
    [InlineData("Jellyfin Web", "Jellyfin Web")]
    [InlineData("jellyfin web", "JELLYFIN WEB")]
    [InlineData("  Jellyfin Web  ", "Jellyfin Web")]
    [InlineData("Jellyfin Web", "  Jellyfin Web\t")]
    public void Matches_ClientCriterion_CaseInsensitiveAndTrimmed(string ruleClient, string requesterClient)
    {
        var rule = Rule(client: ruleClient);

        Assert.True(RuleResolver.Matches(rule, Requester(client: requesterClient)));
    }

    [Fact]
    public void Matches_ClientCriterion_DifferentClient_DoesNotMatch()
    {
        var rule = Rule(client: "Jellyfin Web");

        Assert.False(RuleResolver.Matches(rule, Requester(client: "Jellyfin Android TV")));
    }

    [Fact]
    public void Matches_ClientCriterion_NullRequesterClient_DoesNotMatch()
    {
        var rule = Rule(client: "Jellyfin Web");

        Assert.False(RuleResolver.Matches(rule, Requester(client: null)));
    }

    // ---- Matches: device ----

    [Theory]
    [InlineData("device-1", "device-1")]
    [InlineData("  device-1  ", "device-1")]
    [InlineData("device-1", " device-1 ")]
    public void Matches_DeviceCriterion_TrimmedExactMatch(string ruleDevice, string requesterDevice)
    {
        var rule = Rule(deviceId: ruleDevice);

        Assert.True(RuleResolver.Matches(rule, Requester(deviceId: requesterDevice)));
    }

    [Fact]
    public void Matches_DeviceCriterion_IsCaseSensitive()
    {
        var rule = Rule(deviceId: "ABC123");

        Assert.False(RuleResolver.Matches(rule, Requester(deviceId: "abc123")));
    }

    [Fact]
    public void Matches_DeviceCriterion_NullRequesterDevice_DoesNotMatch()
    {
        var rule = Rule(deviceId: "device-1");

        Assert.False(RuleResolver.Matches(rule, Requester(deviceId: null)));
    }

    // ---- Matches: combined ----

    [Fact]
    public void Matches_AllCriteriaSpecified_AllMustMatch()
    {
        var rule = Rule(userId: _user.ToString(), client: "Jellyfin Web", deviceId: "device-1");

        Assert.True(RuleResolver.Matches(rule, Requester()));
        Assert.False(RuleResolver.Matches(rule, Requester(userId: _otherUser)));
        Assert.False(RuleResolver.Matches(rule, Requester(client: "Other")));
        Assert.False(RuleResolver.Matches(rule, Requester(deviceId: "device-2")));
    }

    [Fact]
    public void Matches_UnspecifiedCriteriaAreIgnored()
    {
        var clientOnly = Rule(client: "Jellyfin Web");

        // User and device differ from anything configured; they must not matter.
        Assert.True(RuleResolver.Matches(clientOnly, Requester(userId: Guid.Empty, deviceId: null)));

        var userAndDevice = Rule(userId: _user.ToString("N"), deviceId: "device-1");
        Assert.True(RuleResolver.Matches(userAndDevice, Requester(client: null)));
    }

    // ---- Resolve ----

    [Fact]
    public void Resolve_NoRules_ReturnsNull()
    {
        Assert.Null(RuleResolver.Resolve([], Requester()));
    }

    [Fact]
    public void Resolve_NothingMatches_ReturnsNull()
    {
        var rules = new[]
        {
            Rule(client: "Other", force: true, maxBitrate: 1000),
            Rule(userId: _otherUser.ToString(), force: true),
            Rule(client: "Jellyfin Web", enabled: false, force: true),
        };

        Assert.Null(RuleResolver.Resolve(rules, Requester()));
    }

    [Fact]
    public void Resolve_MatchingRuleThatNeitherForcesNorCaps_ReturnsNull()
    {
        var rules = new[] { Rule(client: "Jellyfin Web"), Rule(userId: _user.ToString(), maxBitrate: 0) };

        Assert.Null(RuleResolver.Resolve(rules, Requester()));
    }

    [Fact]
    public void Resolve_ForceOnly_ReturnsForceWithoutCap()
    {
        var result = RuleResolver.Resolve([Rule(client: "Jellyfin Web", force: true)], Requester());

        Assert.NotNull(result);
        Assert.True(result.ForceTranscode);
        Assert.Null(result.MaxStreamingBitrate);
    }

    [Fact]
    public void Resolve_CapOnly_ReturnsCapWithoutForce()
    {
        var result = RuleResolver.Resolve([Rule(client: "Jellyfin Web", maxBitrate: 4_000_000)], Requester());

        Assert.NotNull(result);
        Assert.False(result.ForceTranscode);
        Assert.Equal(4_000_000, result.MaxStreamingBitrate);
    }

    [Fact]
    public void Resolve_AnyMatchingRuleForcing_ForcesTranscode()
    {
        var rules = new[]
        {
            Rule(client: "Jellyfin Web", force: false, maxBitrate: 1000),
            Rule(deviceId: "device-1", force: true),
        };

        var result = RuleResolver.Resolve(rules, Requester());

        Assert.NotNull(result);
        Assert.True(result.ForceTranscode);
        Assert.Equal(1000, result.MaxStreamingBitrate);
    }

    [Fact]
    public void Resolve_MultipleCaps_UsesLowestPositive()
    {
        var rules = new[]
        {
            Rule(client: "Jellyfin Web", maxBitrate: 8_000_000),
            Rule(deviceId: "device-1", maxBitrate: 2_000_000),
            Rule(userId: _user.ToString(), maxBitrate: 5_000_000),
        };

        var result = RuleResolver.Resolve(rules, Requester());

        Assert.NotNull(result);
        Assert.Equal(2_000_000, result.MaxStreamingBitrate);
    }

    [Fact]
    public void Resolve_NonMatchingRulesAreIgnored()
    {
        var rules = new[]
        {
            Rule(client: "Jellyfin Web", maxBitrate: 6_000_000),
            Rule(client: "Other", force: true, maxBitrate: 100),
            Rule(client: "Jellyfin Web", enabled: false, force: true, maxBitrate: 100),
        };

        var result = RuleResolver.Resolve(rules, Requester());

        Assert.NotNull(result);
        Assert.False(result.ForceTranscode);
        Assert.Equal(6_000_000, result.MaxStreamingBitrate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Resolve_ZeroOrNegativeCap_ContributesNoCap(int bitrate)
    {
        var rules = new[]
        {
            Rule(client: "Jellyfin Web", maxBitrate: bitrate),
            Rule(deviceId: "device-1", maxBitrate: 3_000_000),
        };

        var result = RuleResolver.Resolve(rules, Requester());

        Assert.NotNull(result);
        Assert.Equal(3_000_000, result.MaxStreamingBitrate);
    }

    [Fact]
    public void Resolve_OnlyNonPositiveCapsAndNoForce_ReturnsNull()
    {
        var rules = new[] { Rule(client: "Jellyfin Web", maxBitrate: -5) };

        Assert.Null(RuleResolver.Resolve(rules, Requester()));
    }

    // ---- ApplyCap ----

    [Theory]
    [InlineData(null, 5000, 5000)]
    [InlineData(0, 5000, 5000)]
    [InlineData(-10, 5000, 5000)]
    [InlineData(3000, 5000, 3000)]
    [InlineData(5000, 5000, 5000)]
    [InlineData(9000, 5000, 5000)]
    public void ApplyCap_ReturnsExpected(int? requested, int cap, int expected)
    {
        Assert.Equal(expected, RuleResolver.ApplyCap(requested, cap));
    }
}
