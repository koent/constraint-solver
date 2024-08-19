using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Solving.Propagators;

public class Constant(int variableIndex, int constant) : IPropagator
{
    private int _variableIndex = variableIndex;

    private int _constant = constant;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var variable = store.Variables[_variableIndex];

        if (variable.TryGetFixedValue(out var value))
        {
            return (value == _constant ? Status.Subsumed : Status.Failed, []);
        }

        if (!variable.Domain().Contains(_constant))
        {
            return (Status.Failed, []);
        }

        variable.Update([_constant]);
        return (Status.Subsumed, [_variableIndex]);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _variableIndex;
    }
}