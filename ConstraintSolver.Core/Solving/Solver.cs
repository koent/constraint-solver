using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving;

public class Solver
{
    private readonly Stack<SearchSpace> _searchSpaces = [];

    public Solver(Model model)
    {
        _searchSpaces.Push(new SearchSpace(model));
    }

    public IEnumerable<Solution> Solve()
    {
        while (_searchSpaces.Count != 0)
        {
            var searchSpace = _searchSpaces.Pop();
            var (propagators, store) = searchSpace.Propagate();
            if (store == null)
            {
                continue;
            }

            if (store.IsSolved)
            {
                yield return new Solution(store);
                continue;
            }

            foreach (var branchIndex in store.GetBranchVariable().BranchIndices().Reverse())
            {
                _searchSpaces.Push(new SearchSpace(store, branchIndex, propagators, searchSpace.Depth + 1));
            }
        }
    }
}