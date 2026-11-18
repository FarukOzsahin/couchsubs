// SPDX-License-Identifier: AGPL-3.0-only

using CouchSubs.Translation;
using Xunit;

namespace CouchSubs.Translation.Tests;

public class GameGlossaryTests
{
    [Theory]
    [InlineData("Grand Theft Auto VI")]
    [InlineData("grand theft auto 6")]
    [InlineData("GTA VI")]
    [InlineData("GTA 6")]
    [InlineData("GTA6")]
    public void Gta6NamesGetTheGlossary(string game)
    {
        var glossary = GameGlossary.For(game);
        Assert.NotNull(glossary);
        Assert.Contains("Lucia", glossary);
        Assert.Contains("Vice City", glossary);
        Assert.Contains("wasted → ÖLDÜN", glossary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Grand Theft Auto V")]
    [InlineData("GTA V")]
    [InlineData("Grand Theft Auto: Vice City")]
    [InlineData("GTA Vice City")]
    [InlineData("Red Dead Redemption 2")]
    public void OtherGamesGetNothing(string? game) => Assert.Null(GameGlossary.For(game));

    [Fact]
    public void UserMessageCarriesTheGlossaryOnlyForGta6()
    {
        var gta6 = AiRequest.UserMessage(["Come on, dude!"], [], "Grand Theft Auto VI");
        Assert.Contains("Glossary for this game", gta6);
        Assert.Contains("dude → ahbap", gta6);

        var gta5 = AiRequest.UserMessage(["Come on, dude!"], [], "Grand Theft Auto V");
        Assert.DoesNotContain("Glossary for this game", gta5);
    }
}
