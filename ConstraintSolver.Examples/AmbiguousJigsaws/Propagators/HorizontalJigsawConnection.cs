using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Core.Solving.SearchSpaces;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

public class HorizontalJigsawConnection(int leftIndex, int rightIndex, int[,] board, Dictionary<int, List<(int, int)>> connectionToPieceAndOrientation) : IPropagator
{
    private readonly int[,] _board = board;
    private readonly int _leftIndex = leftIndex;
    private readonly int _rightIndex = rightIndex;

    private readonly Dictionary<int, List<(int Piece, int Orientation)>> _connectionToPieceAndOrientation = connectionToPieceAndOrientation;

    public (Status, IEnumerable<int>) Invoke(Store store)
    {
        var left = store.Variables[_leftIndex];
        var right = store.Variables[_rightIndex];

        if (left.TryGetFixedValue(out var leftValue) && right.TryGetFixedValue(out var rightValue))
        {
            if (LeftConnection(leftValue) == InvertedRightConnection(rightValue))
            {
                return (Status.Subsumed, []);
            }

            return (Status.Failed, []);
        }

        var leftConnections = left.Domain().Select(LeftConnection).ToHashSet();
        var invertedRightConnections = right.Domain().Select(InvertedRightConnection).ToHashSet();

        var intersection = Enumerable.Intersect(leftConnections, invertedRightConnections).Order().ToList();
        if (intersection.Count == 0)
        {
            return (Status.Failed, []);
        }

        var updatedIndices = new List<int>();

        var leftValues = intersection
            .SelectMany(c => _connectionToPieceAndOrientation[c])
            .Select(po => 4 * po.Piece + (5 - po.Orientation) % 4);
        if (left.Update(leftValues))
        {
            updatedIndices.Add(_leftIndex);
        }

        var rightValues = intersection
            .SelectMany(c => _connectionToPieceAndOrientation[c ^ 1])
            .Select(po => 4 * po.Piece + 3 - po.Orientation);
        if (right.Update(rightValues))
        {
            updatedIndices.Add(_rightIndex);
        }

        return (Status.AtFixpoint, updatedIndices);
    }

    // The connection of interest of the left variable
    // So the connection itself is on the right
    private int LeftConnection(int value)
    {
        var piece = value / 4;
        var rotation = value % 4;
        return _board[piece, (5 - rotation) % 4];
    }

    // The inverse connection of interest of the right variable
    // So the connection itself is on the left
    private int InvertedRightConnection(int value)
    {
        var piece = value / 4;
        var rotation = value % 4;
        return _board[piece, 3 - rotation] ^ 1;
    }

    public IEnumerable<int> VariableIndices()
    {
        yield return _leftIndex;
        yield return _rightIndex;
    }
}
