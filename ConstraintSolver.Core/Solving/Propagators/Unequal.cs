using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Core.Solving.Propagators;

public class Unequal(int leftIndex, int rightIndex) : IPropagator
{
    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var left = store.Variables[leftIndex];
        var right = store.Variables[rightIndex];

        var updatedIndices = new List<int>();

        if (left.TryGetFixedValue(out var leftValue) && right.TryGetFixedValue(out var rightValue))
        {
            return (leftValue == rightValue ? Status.Failed : Status.Subsumed, []);
        }

        if (left.TryGetFixedValue(out leftValue))
        {
            var rightUpdated = right.RemoveFromDomain(leftValue);
            if (rightUpdated)
            {
                updatedIndices.Add(rightIndex);
            }
        }
        else if (right.TryGetFixedValue(out rightValue))
        {
            var leftUpdated = left.RemoveFromDomain(rightValue);
            if (leftUpdated)
            {
                updatedIndices.Add(leftIndex);
            }
        }

        Status status;
        if (left.TryGetFixedValue(out leftValue) && right.TryGetFixedValue(out rightValue))
        {
            status = leftValue == rightValue ? Status.Failed : Status.Subsumed;
        }
        else
        {
            status = Enumerable.Intersect(left.Domain(), right.Domain()).Any() ? Status.AtFixpoint : Status.Subsumed;
        }

        return (status, updatedIndices);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return leftIndex;
        yield return rightIndex;
    }
}