using System.Collections.Generic;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class VerticalJigsawConnection(int topIndex, int bottomIndex, int[,] board) : IPropagator
{
    private readonly int[,] _board = board;
    private readonly int _topIndex = topIndex;
    private readonly int _bottomIndex = bottomIndex;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var top = store.Variables[_topIndex];
        var bottom = store.Variables[_bottomIndex];

        if (top.TryGetFixedValue(out var topValue) && bottom.TryGetFixedValue(out var bottomValue))
        {
            var topPiece = topValue / 4;
            var topOrientation = topValue % 4;
            var bottomPiece = bottomValue / 4;
            var bottomOrientation = bottomValue % 4;

            var topConnection = _board[topPiece, (6 - topOrientation) % 4];
            var bottomConnection = _board[bottomPiece, (4 - bottomOrientation) % 4];

            if ((topConnection ^ bottomConnection) == 1)
            {
                return (Status.Subsumed, []);
            }

            return (Status.Failed, []);
        }

        return (Status.AtFixpoint, []);
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _topIndex;
        yield return _bottomIndex;
    }
}