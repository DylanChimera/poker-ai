using PokerAI.App;
using Xunit;

namespace PokerAI.Tests;

public class CfrSolverTests
{
    private const int Iterations = 20000;

    private static CfrSolver Trained(out double value)
    {
        var solver = new CfrSolver(new KuhnGame());
        value = solver.Train(Iterations);
        return solver;
    }

    private static double Bet(CfrSolver s, string infoSet) =>
        s.Nodes[infoSet].AverageStrategy()[1];     // indice 1 = Bet / Call

    [Fact]
    public void GameValue_IsMinusOneEighteenth()
    {
        Trained(out double value);
        Assert.InRange(value, -0.07, -0.04);       // théorie : -1/18 = -0.0556
    }

    [Fact]
    public void DominatedActions_AreNeverPlayed_AndStrongHandsAlwaysCall()
    {
        var s = Trained(out _);
        Assert.InRange(Bet(s, "Jb"), 0, 0.05);     // J ne paie jamais une mise
        Assert.InRange(Bet(s, "Jpb"), 0, 0.05);
        Assert.InRange(Bet(s, "Kb"), 0.95, 1);     // K paie toujours
        Assert.InRange(Bet(s, "Kpb"), 0.95, 1);
        Assert.InRange(Bet(s, "Kp"), 0.95, 1);     // J2 mise toujours avec K après un check
        Assert.InRange(Bet(s, "Q"), 0, 0.05);      // J1 ne mise jamais avec Q en ouverture
    }

    [Fact]
    public void Strategy_BelongsToTheNashFamily()
    {
        var s = Trained(out _);
        double alpha = Bet(s, "J");                // bluff de J1 avec J, entre 0 et 1/3

        Assert.InRange(alpha, 0, 1.0 / 3 + 0.03);
        Assert.InRange(Bet(s, "K"), 3 * alpha - 0.05, 3 * alpha + 0.05);    // K mise 3 fois plus souvent
        Assert.InRange(Bet(s, "Qpb"), alpha + 1.0 / 3 - 0.05, alpha + 1.0 / 3 + 0.05);
        Assert.InRange(Bet(s, "Jp"), 1.0 / 3 - 0.05, 1.0 / 3 + 0.05);       // bluff de J2
        Assert.InRange(Bet(s, "Qb"), 1.0 / 3 - 0.05, 1.0 / 3 + 0.05);       // call de Q face à une mise
    }
}