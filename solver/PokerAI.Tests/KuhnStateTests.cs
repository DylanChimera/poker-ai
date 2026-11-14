using PokerAI.App;
using Xunit;

namespace PokerAI.Tests;

public class KuhnStateTests
{
    // Cartes : 0 = J, 1 = Q, 2 = K

    [Fact]
    public void BetCall_HigherCardWinsTwo()
    {
        var s = new KuhnState(2, 1, "pbb");
        Assert.True(s.IsTerminal);
        Assert.Equal(new double[] { 2, -2 },
                     s.Returns());
    }

    [Fact]
    public void Fold_BettorWinsOne()
    {
        var s = new KuhnState(0, 2, "bp");   // J1 mise avec J, J2 se couche
        Assert.Equal(new double[] { 1, -1 }, s.Returns());
    }

    [Fact]
    public void CheckCheck_LowerCardLosesOne()
    {
        var s = new KuhnState(0, 1, "pp");
        Assert.Equal(new double[] { -1, 1 }, s.Returns());
    }

    [Theory]
    [InlineData("")]
    [InlineData("p")]
    [InlineData("b")]
    [InlineData("pb")]
    public void NonTerminalHistories(string history)
    {
        Assert.False(new KuhnState(0, 1, history).IsTerminal);
    }

    [Fact]
    public void InformationState_HidesOpponentCard()
    {
        var s = new KuhnState(2, 0, "p");
        Assert.Equal("Kp", s.InformationState(0));
        Assert.Equal("Jp", s.InformationState(1));
    }

    [Fact]
    public void CurrentPlayer_Alternates()
    {
        Assert.Equal(0, new KuhnState(0, 1, "").CurrentPlayer);
        Assert.Equal(1, new KuhnState(0, 1, "p").CurrentPlayer);
        Assert.Equal(0, new KuhnState(0, 1, "pb").CurrentPlayer);
    }
}