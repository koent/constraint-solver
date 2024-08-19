using System.Collections.Generic;

namespace ConstraintSolver.Core.Solving.Propagators;

public interface IPropagator
{
    public (Status, IEnumerable<int>) Invoke(Store store);

    public IEnumerable<int> VariableIndices();
}