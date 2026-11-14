namespace PokerAI.App;

// Un information set : les regrets cumulés et la stratégie cumulée, une case par action.
public class InfoSetNode
{
    public double[] RegretSum { get; }
    public double[] StrategySum { get; }

    public InfoSetNode(int numActions)
    {
        RegretSum = new double[numActions];
        StrategySum = new double[numActions];
    }

    // Regret matching : proportionnel aux regrets positifs (uniforme si aucun).
    public double[] GetStrategy(double ownReach)
    {
        int n = RegretSum.Length;
        var strategy = new double[n];
        double total = 0;
        for (int i = 0; i < n; i++)
        {
            strategy[i] = Math.Max(RegretSum[i], 0);
            total += strategy[i];
        }
        for (int i = 0; i < n; i++)
        {
            strategy[i] = total > 0 ? strategy[i] / total : 1.0 / n;
            StrategySum[i] += ownReach * strategy[i];   // pondérée par notre propre probabilité d'arriver ici
        }
        return strategy;
    }

    public double[] AverageStrategy()
    {
        int n = StrategySum.Length;
        double total = StrategySum.Sum();
        return StrategySum.Select(x => total > 0 ? x / total : 1.0 / n).ToArray();
    }
}

public class CfrSolver
{
    private readonly IGame _game;
    public Dictionary<string, InfoSetNode> Nodes { get; } = new();

    public CfrSolver(IGame game) => _game = game;

    // Renvoie la valeur moyenne du jeu pour le joueur 0.
    public double Train(int iterations)
    {
        double total = 0;
        for (int i = 0; i < iterations; i++)
        {
            var reach = new double[_game.NumPlayers];
            Array.Fill(reach, 1.0);
            total += Cfr(_game.NewInitialState(), reach, 1.0)[0];
        }
        return total / iterations;
    }

    // Renvoie l'utilité espérée de chaque joueur à partir de cet état.
    // reach[i] = probabilité d'arriver ici grâce aux choix du joueur i ; chance = idem pour le hasard.
    private double[] Cfr(IState state, double[] reach, double chance)
    {
        if (state.IsTerminal)
            return state.Returns();

        int n = _game.NumPlayers;
        var nodeUtil = new double[n];

        // Nœud de hasard : on parcourt toutes les issues, pondérées par leur probabilité.
        if (state.CurrentPlayer == Players.Chance)
        {
            foreach (var (action, prob) in state.ChanceOutcomes())
            {
                var u = Cfr(state.Apply(action), reach, chance * prob);
                for (int i = 0; i < n; i++) nodeUtil[i] += prob * u[i];
            }
            return nodeUtil;
        }

        // Nœud de décision.
        int player = state.CurrentPlayer;
        var actions = state.LegalActions();
        string key = state.InformationState(player);

        if (!Nodes.TryGetValue(key, out var node))
        {
            node = new InfoSetNode(actions.Count);
            Nodes[key] = node;
        }

        var strategy = node.GetStrategy(reach[player]);
        var actionUtil = new double[actions.Count][];

        for (int a = 0; a < actions.Count; a++)
        {
            var newReach = (double[])reach.Clone();     // copie du contenu, pas de la référence
            newReach[player] *= strategy[a];
            actionUtil[a] = Cfr(state.Apply(actions[a]), newReach, chance);
            for (int i = 0; i < n; i++) nodeUtil[i] += strategy[a] * actionUtil[a][i];
        }

        // Regret contrefactuel : pondéré par la probabilité que les AUTRES joueurs
        // et le hasard nous amènent ici. Notre propre probabilité est ignorée.
        double counterfactualReach = chance;
        for (int i = 0; i < n; i++)
            if (i != player) counterfactualReach *= reach[i];

        for (int a = 0; a < actions.Count; a++)
            node.RegretSum[a] += counterfactualReach * (actionUtil[a][player] - nodeUtil[player]);

        return nodeUtil;
    }
}

