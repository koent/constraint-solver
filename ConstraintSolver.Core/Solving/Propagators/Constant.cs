using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Solving.Propagators;

public class Constant(int variableIndex, int constant) : IPropagator
{
    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var variable = store.Variables[variableIndex];

        if (variable.TryGetFixedValue(out var value))
        {
            return (value == constant ? Status.Subsumed : Status.Failed, []);
        }

        if (!variable.Domain().Contains(constant))
        {
            return (Status.Failed, []);
        }

        variable.Update([constant]);
        return (Status.Subsumed, [variableIndex]);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return variableIndex;
    }
}