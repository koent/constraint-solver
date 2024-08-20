using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving;

public class Solver
{
    private readonly Stack<SearchSpace> _searchSpaces = [];

    private readonly Statistics _statistics;

    public Solver(Model model)
    {
        _searchSpaces.Push(new SearchSpace(model));
        _statistics = new Statistics();
    }

    public IEnumerable<Solution> Solve()
    {
        _statistics.StartTracking();
        while (_searchSpaces.Count != 0)
        {
            var searchSpace = _searchSpaces.Pop();
            _statistics.Update(searchSpace);
            var (propagators, store) = searchSpace.Propagate();
            if (store == null)
            {
                _statistics.NofFailedPropagations++;
                continue;
            }

            if (store.IsSolved)
            {
                yield return new Solution(store, _statistics.Collect());
                continue;
            }

            foreach (var branchIndex in store.GetBranchVariable().BranchIndices().Reverse())
            {
                _searchSpaces.Push(new SearchSpace(store, branchIndex, propagators, searchSpace.Depth + 1));
            }
        }
        _statistics.StopTracking();
    }
}