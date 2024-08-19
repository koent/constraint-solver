using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class Constant(IVariable variable, int constant) : IConstraint
{
    private readonly string _name = $"{variable} = {constant}";

    public IEnumerable<IVariable> Variables()
    {
        yield return variable;
    }

    public override string ToString() => _name;

    public int ConstantValue => constant;
}