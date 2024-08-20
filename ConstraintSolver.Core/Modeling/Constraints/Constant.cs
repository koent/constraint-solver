using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using ConstantPropagator = ConstraintSolver.Core.Solving.Propagators.Constant;

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

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        yield return new ConstantPropagator(variablesList.IndexOf(variable), constant);
    }    
}