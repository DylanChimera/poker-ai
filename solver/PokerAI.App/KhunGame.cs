namespace PokerAI.App;

public class KuhnGame : IGame
{
    public int NumPlayers => 2;
    public IState NewInitialState() => new KuhnState();   // aucune carte distribuée
}