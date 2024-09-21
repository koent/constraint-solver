using System;

namespace ConstraintSolver.Core.Solving.Statistics;

public class SolutionStatistics
{
    public int SolutionIndex { get; init; }

    public TimeSpan Duration { get; init; }

    public int Depth { get; init; }

    public int MaxDepth { get; init; }

    public int NofExploredSearchSpaces { get; init; }

    public int NofFailedPropagations { get; init; }
}
