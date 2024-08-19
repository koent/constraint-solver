using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class AtMost(IVariable variable, int limit) : IConstraint
{
    private readonly string _name = $"{variable} <= {limit}";

    private readonly int _limit = limit;

    private IVariable _variable = variable;

    public IEnumerable<IVariable> Variables()
    {
        yield return _variable;
    }

    public override string ToString() => _name;

    public int LimitValue => _limit;
}