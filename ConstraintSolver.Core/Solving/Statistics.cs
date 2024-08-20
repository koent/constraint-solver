using System;
using System.Diagnostics;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving;

public class Statistics
{
    private readonly Stopwatch _stopwatch;

    public Statistics()
    {
        _stopwatch = new Stopwatch();
    }

    public TimeSpan Duration;

    public int Depth = 0;

    public int MaxDepth = 0;

    public int NofExploredSearchSpaces = 0;

    public int NofFailedPropagations = 0;

    public void StartTracking()
    {
        _stopwatch.Start();
    }

    public void Update(SearchSpace searchSpace)
    {
        NofExploredSearchSpaces++;
        Depth = searchSpace.Depth;
        MaxDepth = Math.Max(MaxDepth, searchSpace.Depth);
        if (searchSpace.PropagationFailed)
        {
            NofFailedPropagations++;
        }
    }

    public void StopTracking()
    {
        _stopwatch.Stop();
    }

    public Statistics Collect()
    {
        return new Statistics()
        {
            Duration = _stopwatch.Elapsed,
            Depth = Depth,
            MaxDepth = MaxDepth,
            NofExploredSearchSpaces = NofExploredSearchSpaces,
            NofFailedPropagations = NofFailedPropagations
        };
    }
}