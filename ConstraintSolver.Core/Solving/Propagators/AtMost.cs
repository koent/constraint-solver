using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Solving.Propagators;

public class AtMost(int variableIndex, int limit) : IPropagator
{
    private int _variableIndex = variableIndex;

    private int _limit = limit;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var variable = store.Variables[_variableIndex];

        if (variable.TryGetFixedValue(out var value))
        {
            return (value <= _limit ? Status.Subsumed : Status.Failed, []);
        }

        var updated = variable.Update(variable.Domain().Where(v => v <= _limit));

        if (variable.IsUnsatisfiable)
        {
            return (Status.Failed, []);
        }

        return (Status.Subsumed, updated ? [_variableIndex] : []);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _variableIndex;
    }
}