namespace PokerAI.App;

public enum KuhnAction { Pass = 0, Bet = 1 }

public class KuhnState : IState
{
    private readonly int[] _cards;      // 0=J, 1=Q, 2=K ; -1 = pas encore distribuée
    public string History { get; }      // "p" = pass, "b" = bet

    public KuhnState(int card0 = -1, int card1 = -1, string history = "")
    {
        _cards = new[] { card0, card1 };
        History = history;
    }

    public bool IsTerminal => History is "pp" or "bp" or "bb" or "pbp" or "pbb";

    public int CurrentPlayer
    {
        get
        {
            if (IsTerminal) return Players.Terminal;
            if (_cards[0] < 0 || _cards[1] < 0) return Players.Chance;
            return History.Length % 2;
        }
    }

    public IReadOnlyList<int> LegalActions()
    {
        int p = CurrentPlayer;
        if (p == Players.Terminal) return Array.Empty<int>();
        if (p == Players.Chance) return RemainingCards();
        return new[] { (int)KuhnAction.Pass, (int)KuhnAction.Bet };
    }

    public IReadOnlyList<(int Action, double Prob)> ChanceOutcomes()
    {
        if (CurrentPlayer != Players.Chance)
            throw new InvalidOperationException("Pas un nœud de hasard.");
        var remaining = RemainingCards();
        return remaining.Select(c => (c, 1.0 / remaining.Count)).ToList();
    }

    public IState Apply(int action)
    {
        int p = CurrentPlayer;
        if (p == Players.Terminal)
            throw new InvalidOperationException("L'état est terminal.");

        if (p == Players.Chance)   // l'action est la carte distribuée
            return _cards[0] < 0
                ? new KuhnState(action, -1, History)
                : new KuhnState(_cards[0], action, History);

        return new KuhnState(_cards[0], _cards[1],
            History + (action == (int)KuhnAction.Pass ? 'p' : 'b'));
    }

    public double[] Returns()
    {
        if (!IsTerminal)
            throw new InvalidOperationException("L'état n'est pas terminal.");

        if (History == "bp") return new double[] { 1, -1 };
        if (History == "pbp") return new double[] { -1, 1 };

        int amount = History == "pp" ? 1 : 2;
        return _cards[0] > _cards[1]
            ? new double[] { amount, -amount }
            : new double[] { -amount, amount };
    }

    public string InformationState(int player) =>
        (_cards[player] < 0 ? '?' : "JQK"[_cards[player]]) + History;

    private List<int> RemainingCards() =>
        Enumerable.Range(0, 3).Where(c => c != _cards[0] && c != _cards[1]).ToList();
}