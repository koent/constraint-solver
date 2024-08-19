using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class Unequal(IVariable left, IVariable right) : IConstraint
{
    private string _name = $"{left} != {right}";

    private IVariable _left = left;
    
    private IVariable _right = right;

    public IEnumerable<IVariable> Variables()
    {
        yield return _left;
        yield return _right;
    }

    public override string ToString() => _name;
}