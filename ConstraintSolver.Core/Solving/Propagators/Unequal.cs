using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Solving.Propagators;

public class Unequal(int leftIndex, int rightIndex) : IPropagator
{
    private int _leftIndex = leftIndex;

    private int _rightIndex = rightIndex;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var left = store.Variables[_leftIndex];
        var right = store.Variables[_rightIndex];

        var updatedIndices = new List<int>();

        // if (left.Status() == Variables.Status.Fixed && right.Status() == Variables.Status.Fixed)
        // {
        //     return (left.Domain().Single() == right.Domain().Single() ? Status.Failed : Status.Subsumed, []);
        // }
        if (left.TryGetFixedValue(out var leftValue) && right.TryGetFixedValue(out var rightValue))
        {
            return (leftValue == rightValue ? Status.Failed : Status.Subsumed, []);
        }

        // if (left.Status() == Variables.Status.Fixed)
        // {
        //     var leftValue = left.Domain().Single();
        if (left.TryGetFixedValue(out leftValue))
        {
            var rightUpdated = right.RemoveFromDomain(leftValue);
            if (rightUpdated)
            {
                updatedIndices.Add(_rightIndex);
            }
        }
        // else if (right.Status() == Variables.Status.Fixed)
        // {
        //     var rightValue = right.Domain().Single();
        else if (right.TryGetFixedValue(out rightValue))
        {
            var leftUpdated = left.RemoveFromDomain(rightValue);
            if (leftUpdated)
            {
                updatedIndices.Add(_leftIndex);
            }
        }

        Status status;
        // if (left.Status() == Variables.Status.Fixed && right.Status() == Variables.Status.Fixed)
        // {
        //     status = left.Domain().Single() == right.Domain().Single() ? Status.Failed : Status.Subsumed;
        // }
        if (left.TryGetFixedValue(out leftValue) && right.TryGetFixedValue(out rightValue))
        {
            // return (leftValue3 == rightValue3 ? Status.Failed : Status.Subsumed, []);
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
        yield return _leftIndex;
        yield return _rightIndex;
    }
}