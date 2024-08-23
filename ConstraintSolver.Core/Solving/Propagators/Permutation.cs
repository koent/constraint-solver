using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving.Propagators;

public class Permutation(IEnumerable<int> variables) : IPropagator
{
    private readonly List<int> _variableIndices = variables.Order().ToList();

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var variables = _variableIndices.Select(i => store.Variables[i]);
        if (variables.All(v => v.IsFixed)) return (Status.Subsumed, []);


        var values = variables.SelectMany(v => v.Domain()).ToHashSet();
        if (values.Count < _variableIndices.Count)
        {
            return (Status.Failed, []);
        }

        return (Status.AtFixpoint, []);
    }

    public IEnumerable<int> VariableIndices() => _variableIndices;
}