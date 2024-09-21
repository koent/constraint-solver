using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class PiecewiseUnequal(int leftIndex, int rightIndex) : IPropagator
{
    private readonly int _leftIndex = Math.Min(leftIndex, rightIndex);
    private readonly int _rightIndex = Math.Max(leftIndex, rightIndex);

    private const int NofOrientations = 4;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var orientations = Enumerable.Range(0, NofOrientations).ToList();

        var left = store.Variables[_leftIndex];
        var right = store.Variables[_rightIndex];

        var updatedIndices = new List<int>();

        var leftPieces = left.Domain().Select(v => v / NofOrientations).ToHashSet().Order().ToList();
        var rightPieces = right.Domain().Select(v => v / NofOrientations).ToHashSet().Order().ToList();

        if (leftPieces.Count == 1 && rightPieces.Count == 1)
        {
            return (leftPieces.Single() == rightPieces.Single() ? Status.Failed : Status.Subsumed, []);
        }

        if (leftPieces.Count == 1)
        {
            var leftPiece = leftPieces.Single();
            var rightUpdated = false;
            foreach (var o in orientations)
            {
                rightUpdated |= right.RemoveFromDomain(4 * leftPiece + o);
            }
            if (rightUpdated)
            {
                updatedIndices.Add(_rightIndex);
            }
        }
        else if (rightPieces.Count == 1)
        {
            var rightPiece = rightPieces.Single();
            var leftUpdated = false;
            foreach (var o in orientations)
            {
                leftUpdated |= left.RemoveFromDomain(4 * rightPiece + o);
            }
            if (leftUpdated)
            {
                updatedIndices.Add(_leftIndex);
            }
        }

        Status status;

        leftPieces = left.Domain().Select(v => v / NofOrientations).ToHashSet().Order().ToList();
        rightPieces = right.Domain().Select(v => v / NofOrientations).ToHashSet().Order().ToList();

        if (leftPieces.Count == 1 && rightPieces.Count == 1)
        {
            status = leftPieces.Single() == rightPieces.Single() ? Status.Failed : Status.Subsumed;
        }
        else
        {
            status = Enumerable.Intersect(leftPieces, rightPieces).Any() ? Status.AtFixpoint : Status.Subsumed;
        }

        return (status, updatedIndices);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _leftIndex;
        yield return _rightIndex;
    }
}