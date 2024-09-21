using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.SearchSpaces;
using ConstraintSolver.Core.Solving.Statistics;

namespace ConstraintSolver.Core.Solving;

public class Solver
{
    private readonly Stack<SearchSpace> _searchSpaces = [];

    private readonly StatisticsCollector _statisticsCollector = new StatisticsCollector();

    private GlobalStatistics _globalStatistics;

    private readonly int? _maxNofSolutions;

    public Solver(Model model, int? maxNofSolutions = null)
    {
        _searchSpaces.Push(new SearchSpace(model));
        _maxNofSolutions = maxNofSolutions;
    }

    public IEnumerable<Solution> Solve()
    {
        _statisticsCollector.StartTracking();
        while (_searchSpaces.Count != 0 && (!_maxNofSolutions.HasValue || _statisticsCollector.NofSolutions < _maxNofSolutions.Value))
        {
            var searchSpace = _searchSpaces.Pop();

            searchSpace.Propagate();

            _statisticsCollector.Update(searchSpace);

            if (searchSpace.PropagationFailed)
            {
                continue;
            }

            var store = searchSpace.Store;
            if (store.IsSolved)
            {
                yield return new Solution(store, _statisticsCollector.Collect());
                continue;
            }

            var branchVariableIndex = store.GetBranchVariableIndex();
            foreach (var branchIndex in store.Variables[branchVariableIndex].BranchIndices().Reverse())
            {
                _searchSpaces.Push(new SearchSpace(searchSpace, branchVariableIndex, branchIndex));
            }
        }
        _globalStatistics = _statisticsCollector.StopTracking();
    }

    public void PrintStatistics()
    {
        Console.WriteLine($"Total duration: {_globalStatistics.Duration}");

        if (_maxNofSolutions.HasValue)
        {
            Console.WriteLine($"Number of found solutions (max): {_globalStatistics.NofFoundSolutions} ({_maxNofSolutions.Value})");
        }
        else
        {
            Console.WriteLine($"Total number of solutions: {_globalStatistics.NofFoundSolutions}");
        }

        Console.WriteLine($"Max depth: {_globalStatistics.MaxDepth}");
    }
}