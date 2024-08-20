using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Modeling.Constraints;

public interface IConstraint
{
    public IEnumerable<IVariable> Variables();

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList);
}