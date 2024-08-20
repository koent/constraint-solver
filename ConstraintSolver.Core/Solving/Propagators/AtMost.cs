using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving.Propagators;

public class AtMost(int variableIndex, int limit) : IPropagator
{
    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var variable = store.Variables[variableIndex];

        if (variable.TryGetFixedValue(out var value))
        {
            return (value <= limit ? Status.Subsumed : Status.Failed, []);
        }

        var updated = variable.Update(variable.Domain().Where(v => v <= limit));

        if (variable.IsUnsatisfiable)
        {
            return (Status.Failed, []);
        }

        return (Status.Subsumed, updated ? [variableIndex] : []);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return variableIndex;
    }
}