using System.Collections.Generic;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class HorizontalJigsawConnection(int leftIndex, int rightIndex, int[,] board) : IPropagator
{
    private readonly int[,] _board = board;
    private readonly int _leftIndex = leftIndex;
    private readonly int _rightIndex = rightIndex;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var left = store.Variables[_leftIndex];
        var right = store.Variables[_rightIndex];

        if (left.TryGetFixedValue(out var leftValue) && right.TryGetFixedValue(out var rightValue))
        {
            var leftPiece = leftValue / 4;
            var leftOrientation = leftValue % 4;
            var rightPiece = rightValue / 4;
            var rightOrientation = rightValue % 4;

            var leftConnection = _board[leftPiece, (5 - leftOrientation) % 4];
            var rightConnection = _board[rightPiece, 3 - rightOrientation];

            if ((leftConnection ^ rightConnection) == 1)
            {
                return (Status.Subsumed, []);
            }

            return (Status.Failed, []);
        }

        return (Status.AtFixpoint, []);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _leftIndex;
        yield return _rightIndex;
    }
}