using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Modeling.Constraints;

public class Permutation(IEnumerable<IVariable> variables) : IConstraint
{
    private readonly List<IVariable> _variables = variables.ToList();

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        var variableIndices = _variables.Select(v => variablesList.IndexOf(v)).ToList();

        for (int i = 0; i < _variables.Count - 1; i++)
        {
            for (int j = i + 1; j < _variables.Count; j++)
            {
                yield return new Solving.Propagators.Unequal(variableIndices[i], variableIndices[j]);
            }
        }
    }

    public IEnumerable<IVariable> Variables() => _variables;
}