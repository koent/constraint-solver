using System.Collections.Generic;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving.Propagators;

public interface IPropagator
{
    public (Status, IEnumerable<int>) Invoke(Store store);

    public IEnumerable<int> VariableIndices();
}