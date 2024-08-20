using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using AtMostPropagator = ConstraintSolver.Core.Solving.Propagators.AtMost;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class AtMost(IVariable variable, int limit) : IConstraint
{
    private readonly string _name = $"{variable} <= {limit}";

    public IEnumerable<IVariable> Variables()
    {
        yield return variable;
    }

    public override string ToString() => _name;

    public int LimitValue => limit;

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        yield return new AtMostPropagator(variablesList.IndexOf(variable), limit);
    }
}