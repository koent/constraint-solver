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

        var updatedVariableIndices = new HashSet<int>();
        var changed = true;
        while (changed)
        {
            changed = false;
            var valueToVariableIndices = values.ToDictionary(v => v, _ => new List<int>());
            foreach (var variableIndex in _variableIndices)
            {
                foreach (var value in store.Variables[variableIndex].Domain())
                {
                    valueToVariableIndices[value].Add(variableIndex);
                }
            }

            foreach (var (value, variableIndices) in valueToVariableIndices)
            {
                if (variableIndices.Count == 1)
                {
                    var variableIndex = variableIndices.Single();
                    if (store.Variables[variableIndex].Update([value]))
                    {
                        changed = true;
                        updatedVariableIndices.Add(variableIndex);
                    }
                }
            }
        }

        return (variables.All(v => v.IsFixed) ? Status.Subsumed : Status.AtFixpoint, updatedVariableIndices);
    }

    public IEnumerable<int> VariableIndices() => _variableIndices;
}