namespace PokerAI.App;

public static class Players
{
    public const int Chance = -1;     // nœud de hasard (distribution des cartes)
    public const int Terminal = -4;   // partie finie (mêmes valeurs qu'OpenSpiel)
}

public interface IState
{
    int CurrentPlayer { get; }        // 0, 1, Players.Chance ou Players.Terminal
    bool IsTerminal { get; }

    // Actions jouables. Pour un nœud de hasard : les issues possibles (ex. cartes restantes).
    IReadOnlyList<int> LegalActions();

    // Seulement pour un nœud de hasard : chaque issue avec sa probabilité.
    IReadOnlyList<(int Action, double Prob)> ChanceOutcomes();

    IState Apply(int action);         // renvoie le nouvel état
    double[] Returns();               // gains de chaque joueur (état terminal)
    string InformationState(int player);
}

public interface IGame
{
    int NumPlayers { get; }
    IState NewInitialState();
}