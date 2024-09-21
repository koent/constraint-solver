using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class VerticalJigsawConnection(int topIndex, int bottomIndex, int[,] board, Dictionary<int, List<(int, int)>> connectionToPieceAndOrientation) : IPropagator
{
    private readonly int[,] _board = board;
    private readonly int _topIndex = topIndex;
    private readonly int _bottomIndex = bottomIndex;

    private readonly Dictionary<int, List<(int Piece, int Orientation)>> _connectionToPieceAndOrientation = connectionToPieceAndOrientation;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var top = store.Variables[_topIndex];
        var bottom = store.Variables[_bottomIndex];

        if (top.TryGetFixedValue(out var topValue) && bottom.TryGetFixedValue(out var bottomValue))
        {
            if (TopConnection(topValue) == InvertedBottomConnection(bottomValue))
            {
                return (Status.Subsumed, []);
            }

            return (Status.Failed, []);
        }

        var topConnections = top.Domain().Select(TopConnection).ToHashSet();
        var invertedBottomConnections = bottom.Domain().Select(InvertedBottomConnection).ToHashSet();

        var intersection = Enumerable.Intersect(topConnections, invertedBottomConnections).Order().ToList();
        if (intersection.Count == 0)
        {
            return (Status.Failed, []);
        }

        var updatedIndices = new List<int>();

        var topValues = intersection
            .SelectMany(c => _connectionToPieceAndOrientation[c])
            .Select(po => 4 * po.Piece + (6 - po.Orientation) % 4);
        if (top.Update(topValues))
        {
            updatedIndices.Add(_topIndex);
        }

        var bottomValues = intersection
            .SelectMany(c => _connectionToPieceAndOrientation[c ^ 1])
            .Select(po => 4 * po.Piece + (4 - po.Orientation) % 4);
        if (bottom.Update(bottomValues))
        {
            updatedIndices.Add(_bottomIndex);
        }

        return (Status.AtFixpoint, []);
    }

    // The connection of interest of the top variable
    // So the connection itself is on the bottom
    private int TopConnection(int value)
    {
        var piece = value / 4;
        var rotation = value % 4;
        return _board[piece, (6 - rotation) % 4];
    }

    // The inverse connection of interest of the bottom variable
    // So the connection itself is on the top
    private int InvertedBottomConnection(int value)
    {
        var piece = value / 4;
        var rotation = value % 4;
        return _board[piece, (4 - rotation) % 4] ^ 1;
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _topIndex;
        yield return _bottomIndex;
    }
}
