using System;
using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Constraints;

public class JigsawConnection(IVariable left, IVariable right, bool vertical) : IConstraint
{
    private static int[,] _board;
    private static Dictionary<int, List<(int, int)>> _connectionToPieceAndOrientation;

    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        if (_connectionToPieceAndOrientation == null)
        {
            throw new InvalidOperationException("Board must be set first");
        }

        yield return vertical
         ? new VerticalJigsawConnection(variablesList.IndexOf(left), variablesList.IndexOf(right), _board, _connectionToPieceAndOrientation)
         : new HorizontalJigsawConnection(variablesList.IndexOf(left), variablesList.IndexOf(right), _board, _connectionToPieceAndOrientation);
    }

    public static void SetBoard(int[,] board)
    {
        _board = board;

        _connectionToPieceAndOrientation = [];
        for (var piece = 0; piece < board.GetLength(0); piece++)
        {
            for (var orientation = 0; orientation < 4; orientation++)
            {
                var connection = _board[piece, orientation];
                if (connection < 0) continue;

                if (_connectionToPieceAndOrientation.TryGetValue(connection, out List<(int, int)> list))
                {
                    list.Add((piece, orientation));
                }
                else
                {
                    _connectionToPieceAndOrientation[connection] = [(piece, orientation)];
                }
            }
        }
    }

    public IEnumerable<IVariable> Variables()
    {
        yield return left;
        yield return right;
    }
}