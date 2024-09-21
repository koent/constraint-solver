using System;
using System.Diagnostics;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving.Statistics;

public class StatisticsCollector
{
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private int _depth = 0;

    private int _maxDepth = 0;

    private int _nofExploredSearchSpaces = 0;

    private int _nofFailedPropagations = 0;

    public int NofSolutions { get; private set; }

    public void StartTracking()
    {
        _stopwatch.Start();
    }

    public void Update(SearchSpace searchSpace)
    {
        _nofExploredSearchSpaces++;
        _depth = searchSpace.Depth;
        _maxDepth = Math.Max(_maxDepth, _depth);
        if (searchSpace.PropagationFailed)
        {
            _nofFailedPropagations++;
        }
    }

    public SolutionStatistics Collect()
    {
        return new SolutionStatistics()
        {
            SolutionIndex = NofSolutions++,
            Duration = _stopwatch.Elapsed,
            Depth = _depth,
            MaxDepth = _maxDepth,
            NofExploredSearchSpaces = _nofExploredSearchSpaces,
            NofFailedPropagations = _nofFailedPropagations
        };
    }

    public GlobalStatistics StopTracking()
    {
        _stopwatch.Stop();

        return new GlobalStatistics()
        {
            Duration = _stopwatch.Elapsed,
            NofFoundSolutions = NofSolutions,
            MaxDepth = _maxDepth
        };
    }
}
