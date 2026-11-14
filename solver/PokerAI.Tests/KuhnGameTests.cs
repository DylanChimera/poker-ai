using PokerAI.App;
using Xunit;

namespace PokerAI.Tests;

public class KuhnGameTests
{
    [Fact]
    public void InitialState_IsChanceNode_WithThreeEqualOutcomes()
    {
        IState s = new KuhnGame().NewInitialState();
        Assert.Equal(Players.Chance, s.CurrentPlayer);
        var outcomes = s.ChanceOutcomes();
        Assert.Equal(3, outcomes.Count);
        Assert.All(outcomes, o => Assert.Equal(1.0 / 3, o.Prob, 10));
    }

    [Fact]
    public void SecondDeal_HasTwoOutcomes_AndExcludesFirstCard()
    {
        IState s = new KuhnGame().NewInitialState().Apply(2);   // J0 reçoit le K
        Assert.Equal(Players.Chance, s.CurrentPlayer);
        Assert.Equal(new[] { 0, 1 }, s.LegalActions());
    }

    [Fact]
    public void AfterDeal_PlayerZeroActs_WithPassAndBet()
    {
        IState s = new KuhnGame().NewInitialState().Apply(2).Apply(0);
        Assert.Equal(0, s.CurrentPlayer);
        Assert.Equal(new[] { 0, 1 }, s.LegalActions());
    }

    [Fact]
    public void FullTree_Has30Leaves_And12InformationSets()
    {
        var infoSets = new HashSet<string>();
        int leaves = Walk(new KuhnGame().NewInitialState(), infoSets);
        Assert.Equal(30, leaves);        // 6 distributions x 5 fins de partie
        Assert.Equal(12, infoSets.Count);
    }

    private static int Walk(IState s, HashSet<string> infoSets)
    {
        if (s.IsTerminal) return 1;
        int p = s.CurrentPlayer;
        if (p != Players.Chance) infoSets.Add(s.InformationState(p));

        int leaves = 0;
        foreach (int a in s.LegalActions())
            leaves += Walk(s.Apply(a), infoSets);
        return leaves;
    }
}