using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using PiecewiseUnequalPropagator = ConstraintSolver.Examples.AmbiguousJigsaws.Propagators.PiecewiseUnequal;
using PiecewisePermutationPropagator = ConstraintSolver.Examples.AmbiguousJigsaws.Propagators.PiecewisePermutation;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Constraints;

public class PiecewisePermutation(IEnumerable<IVariable> variables) : IConstraint
{
    private readonly List<IVariable> _variables = variables.ToList();

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        var variableIndices = _variables.Select(v => variablesList.IndexOf(v)).ToList();

        for (int i = 0; i < _variables.Count - 1; i++)
        {
            for (int j = i + 1; j < _variables.Count; j++)
            {
                yield return new PiecewiseUnequalPropagator(variableIndices[i], variableIndices[j]);
            }
        }

        if (variableIndices.Count > 2)
        {
            yield return new PiecewisePermutationPropagator(variableIndices);
        }
    }

    public IEnumerable<IVariable> Variables() => _variables;
}
