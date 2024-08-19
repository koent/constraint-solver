using System.Collections.Generic;

namespace ConstraintSolver.Core.Modeling.Variables;

public interface IVariable
{
    public IEnumerable<int> Domain();
}