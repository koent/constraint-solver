using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

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
}