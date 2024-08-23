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
            Dictionary<int, List<int>> valueToVariableIndex = [];
            foreach (var variableIndex in _variableIndices)
            {
                foreach (var value in store.Variables[variableIndex].Domain())
                {
                    if (valueToVariableIndex.TryGetValue(value, out List<int> indices))
                    {
                        indices.Add(variableIndex);
                    }
                    else
                    {
                        valueToVariableIndex[value] = [variableIndex];
                    }
                }
            }

            foreach (var (value, variableIndices) in valueToVariableIndex)
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

        return (Status.AtFixpoint, updatedVariableIndices);
    }

    public IEnumerable<int> VariableIndices() => _variableIndices;
}