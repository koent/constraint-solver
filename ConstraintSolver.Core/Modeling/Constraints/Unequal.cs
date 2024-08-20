using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using UnequalPropagator = ConstraintSolver.Core.Solving.Propagators.Unequal;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class Unequal(IVariable left, IVariable right) : IConstraint
{
    private readonly string _name = $"{left} != {right}";

    public IEnumerable<IVariable> Variables()
    {
        yield return left;
        yield return right;
    }

    public override string ToString() => _name;

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        yield return new UnequalPropagator(variablesList.IndexOf(left), variablesList.IndexOf(right));
    }    
}