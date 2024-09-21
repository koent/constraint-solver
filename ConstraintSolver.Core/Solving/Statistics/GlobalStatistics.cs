using System;

namespace ConstraintSolver.Core.Solving.Statistics;

public class GlobalStatistics
{
    public TimeSpan Duration { get; init; }

    public int NofFoundSolutions { get; init; }

    public int MaxDepth { get; init; }
}
