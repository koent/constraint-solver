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

            searchSpace.Propagate();

            _statistics.Update(searchSpace);

            if (searchSpace.PropagationFailed)
            {
                continue;
            }

            var store = searchSpace.Store;
            if (store.IsSolved)
            {
                yield return new Solution(store, _statistics.Collect());
                continue;
            }

            var branchVariableIndex = store.GetBranchVariableIndex();
            foreach (var branchIndex in store.Variables[branchVariableIndex].BranchIndices().Reverse())
            {
                _searchSpaces.Push(new SearchSpace(searchSpace, branchVariableIndex, branchIndex));
            }
        }
        _statistics.StopTracking();
    }
}