using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;
using ConstraintSolver.Core.Solving.Variables;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class PiecewisePermutation(IEnumerable<int> variableIndices) : IPropagator
{
    private readonly List<int> _variableIndices = variableIndices.Order().ToList();

    private const int NofOrientations = 4;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var orientations = Enumerable.Range(0, NofOrientations).ToList();

        var variables = _variableIndices.Select(i => store.Variables[i]);
        if (variables.All(IsPiecewiseFixed)) return (Status.Subsumed, []);

        var values = variables.SelectMany(v => v.Domain().Select(v => v / NofOrientations)).ToHashSet();
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
                foreach (var value in store.Variables[variableIndex].Domain().Select(v => v / NofOrientations).ToHashSet())
                {
                    valueToVariableIndices[value].Add(variableIndex);
                }
            }

            foreach (var (value, variableIndices) in valueToVariableIndices)
            {
                if (variableIndices.Count == 1)
                {
                    var variableIndex = variableIndices.Single();
                    if (store.Variables[variableIndex].Update(orientations.Select(o => NofOrientations * value + o)))
                    {
                        changed = true;
                        updatedVariableIndices.Add(variableIndex);
                    }
                }
            }
        }

        return (variables.All(IsPiecewiseFixed) ? Status.Subsumed : Status.AtFixpoint, updatedVariableIndices);
    }

    private static bool IsPiecewiseFixed(Variable variable) => variable.IsFixed || variable.Domain().Select(v => v / NofOrientations).ToHashSet().Count == 1;

    public IEnumerable<int> VariableIndices() => _variableIndices;
}