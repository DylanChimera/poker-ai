using PokerAI.App;

var solver = new CfrSolver(new KuhnGame());
double value = solver.Train(20000);
Console.WriteLine($"Valeur du jeu pour J1 : {value:F4}  (théorie : -0.0556)\n");

foreach (var (key, node) in solver.Nodes.OrderBy(kv => kv.Key.Length).ThenBy(kv => kv.Key, StringComparer.Ordinal))
    Console.WriteLine($"{key,-5} -> bet/call = {node.AverageStrategy()[1]:F3}");