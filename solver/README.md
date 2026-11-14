# PokerAI

A poker solver in C# (.NET 11). It computes a Nash equilibrium strategy with **CFR** (Counterfactual Regret Minimization), starting with the simplest poker game: **Kuhn poker**.

## What has been done

### 1. A generic game abstraction
`IGame` and `IState` describe an imperfect-information game as a tree. They follow OpenSpiel's conventions: `Players.Chance = -1` and `Players.Terminal = -4`. The solver only sees these interfaces and has no dependency on Kuhn poker, so adding another game (Leduc, simplified Hold'em…) means implementing these two interfaces.

### 2. Kuhn poker rules
- 3-card deck (J, Q, K), 2 players, 1 card each, ante of 1.
- Actions: `Pass` (check / fold) and `Bet` (bet / call).
- The deal is modelled as **two successive chance nodes**. Each remaining card has the same probability.
- Terminal histories: `pp`, `bp`, `bb`, `pbp`, `pbb`.
  - Fold (`bp`, `pbp`): the player who bet wins 1.
  - Showdown: 1 after `pp`, 2 after a called bet.
- A player's *information state* is their card followed by the public history (for example `Kp`). The opponent's card stays hidden.

### 3. The CFR solver (vanilla)
- Full tree traversal on every iteration, enumerating every chance outcome.
- One `InfoSetNode` per information set. It accumulates:
  - **regrets**, weighted by the counterfactual reach probability (opponents × chance);
  - **strategy**, weighted by the player's own reach probability.
- The current strategy comes from **regret matching** (proportional to positive regrets, uniform otherwise).
- The final strategy is the **average strategy** (`AverageStrategy`), which is the one that converges to equilibrium.
- `Train(n)` returns the average game value for player 0.

### 4. A demo program
`Program.cs` trains the solver for 20,000 iterations. It then prints the game value (theory: −1/18 ≈ −0.0556) and, for each information set, the probability of bet/call.

### 5. Unit tests (xUnit)
| File | What is checked |
|---|---|
| `KuhnStateTests` | Terminal payoffs (call, fold, check-check), non-terminal histories, opponent card hidden in the information state, players alternating |
| `KuhnGameTests` | Initial state is a chance node with 3 equally likely outcomes, second deal excludes the first card, full tree has **30 leaves** and **12 information sets** |
| `CfrSolverTests` | Game value close to −1/18, dominated actions never played (J never calls, K always calls…), strategy inside the known **family of Nash equilibria** (bluff parameter α ∈ [0, 1/3], K bets 3α, Q calls α + 1/3, etc.) |

## Architecture

```
PokerAI/
├── PokerAI.slnx                 # solution (.slnx format)
├── PokerAI.App/                 # console application (net11.0)
│   ├── Interfaces.cs            # IGame, IState, Players constants (Chance / Terminal)
│   ├── KhunGame.cs              # KuhnGame : IGame (2 players, initial state)
│   ├── KuhnState.cs             # KuhnState : IState (rules, payoffs, information states)
│   ├── CfrSolver.cs             # InfoSetNode (regrets, regret matching) + CfrSolver
│   └── Program.cs               # training and strategy printout
└── PokerAI.Tests/               # xUnit tests, references PokerAI.App
    ├── KuhnStateTests.cs
    ├── KuhnGameTests.cs
    └── CfrSolverTests.cs
```

### Flow

```
KuhnGame ──NewInitialState()──► KuhnState (chance node)
                                    │ Apply(card) ×2
                                    ▼
                                KuhnState (P0/P1 decision) ──Apply(Pass|Bet)──► … ──► terminal state ──Returns()
                                    ▲
CfrSolver.Train(n) ── Cfr(state, reach, chance) walks the tree through IState only
        │
        └── Nodes : Dictionary<string, InfoSetNode>   (key = the player's InformationState)
                    └── AverageStrategy()  ≈ Nash equilibrium
```

### Design choices
- **Immutable states**: `Apply` returns a new state, so the CFR recursion never has to undo a move.
- **Information state as a string**: it is used as the dictionary key, simple and readable (`"Qpb"`).
- **Per-player utility vectors** (`double[]`): the solver code is written for `NumPlayers` players, not hard-coded to 2. Kuhn poker itself is 2-player, and CFR's convergence guarantee to a Nash equilibrium only holds for two-player zero-sum games.

## Usage

```bash
dotnet run --project PokerAI.App     # train and print the strategy
dotnet test                          # run the tests
```

Requirement: .NET 11 SDK.

## Next steps
- Rename `KhunGame.cs` to `KuhnGame.cs` (the class inside is already named `KuhnGame`).
- Compute **exploitability** to measure convergence, instead of only comparing the game value with theory.
- Faster-converging variants: **CFR+** (regrets floored at zero, weighted averaging) and **Monte Carlo CFR**, which samples part of the tree each iteration (chance sampling, external sampling, outcome sampling) instead of traversing it fully.
- Larger games: Leduc poker, then an abstracted Hold'em.
