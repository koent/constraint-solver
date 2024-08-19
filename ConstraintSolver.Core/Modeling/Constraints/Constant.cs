using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class Constant(IVariable variable, int constant) : IConstraint
{
    private readonly string _name = $"{variable} = {constant}";

    private readonly int _constant = constant;

    private IVariable _variable = variable;

    public IEnumerable<IVariable> Variables()
    {
        yield return _variable;
    }

    public override string ToString() => _name;

    public int ConstantValue => _constant;
}