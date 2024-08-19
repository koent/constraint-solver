using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Modeling.Constraints;

public interface IConstraint
{
    public IEnumerable<IVariable> Variables();
}