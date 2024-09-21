using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving;
using ConstraintSolver.Examples.AmbiguousJigsaws.Constraints;

namespace ConstraintSolver.Examples.AmbiguousJigsaws;

public class AmbiguousJigsaw : Model
{
    private enum PieceType { Corner, Edge, Inner };

    private readonly int _height, _width;

    private readonly int[,] _board;

    private readonly IVariable[,] _variables;

    public AmbiguousJigsaw(InputData inputData) : this(inputData.Height, inputData.Width, inputData.VerticalEdges, inputData.HorizontalEdges) { }

    public AmbiguousJigsaw(int height, int width, int[,] verticalEdges, int[,] horizontalEdges)
    {
        Validate(height, width, verticalEdges, horizontalEdges);

        _height = height;
        _width = width;

        // Create variables
        _variables = new IVariable[height, width];

        var cornerPieceDomains = Enumerable.Range(0, 4).Select(CornerPieceDomain).ToList();
        var edgePieceDomains = Enumerable.Range(0, 4).Select(EdgePieceDomain).ToList();
        var innerPieceDomain = InnerPieceDomain();

        List<IVariable> cornerPieceVariables = [];
        List<IVariable> edgePieceVariables = [];
        List<IVariable> innerPieceVariables = [];

        for (var row = 0; row < height; row++)
        {
            for (var col = 0; col < width; col++)
            {
                var (pieceType, orientation) = GetPieceTypeAndOrientation(row, col);

                _variables[row, col] = AddVariable(new ExplicitDomain($"J_{row + 1}_{col + 1}", pieceType switch
                {
                    PieceType.Corner => cornerPieceDomains[orientation],
                    PieceType.Edge => edgePieceDomains[orientation],
                    _ => innerPieceDomain
                }));

                switch (pieceType)
                {
                    case PieceType.Corner:
                        cornerPieceVariables.Add(_variables[row, col]);
                        break;
                    case PieceType.Edge:
                        edgePieceVariables.Add(_variables[row, col]);
                        break;
                    case PieceType.Inner:
                        innerPieceVariables.Add(_variables[row, col]);
                        break;
                }
            }
        }

        // All corner pieces different
        AddConstraint(new PiecewisePermutation(cornerPieceVariables));

        // All edge pieces different
        AddConstraint(new PiecewisePermutation(edgePieceVariables));

        // All inner pieces different
        AddConstraint(new PiecewisePermutation(innerPieceVariables));

        // Connection constraints
        _board = new int[_width * _height, 4];
        for (var col = 0; col < _width; col++)
        {
            for (var row = 0; row < _height; row++)
            {
                var index = _width * row + col;
                _board[index, 0] = row == 0 ? 0 : horizontalEdges[row - 1, col] ^ 1;
                _board[index, 1] = col == _width - 1 ? 0 : verticalEdges[row, col];
                _board[index, 2] = row == _height - 1 ? 0 : horizontalEdges[row, col];
                _board[index, 3] = col == 0 ? 0 : verticalEdges[row, col - 1] ^ 1;
            }
        }

        for (var col = 0; col < _width; col++)
        {
            for (var row = 0; row < _height - 1; row++)
            {
                AddConstraint(new JigsawConnection(_variables[row, col], _variables[row + 1, col], true, _board));
            }
        }

        for (var col = 0; col < _width - 1; col++)
        {
            for (var row = 0; row < _height; row++)
            {
                AddConstraint(new JigsawConnection(_variables[row, col], _variables[row, col + 1], false, _board));
            }
        }

        // Symmetry breaking, force top left piece in top left
        AddConstraint(new Constant(_variables[0, 0], 0));

        // Calculate propagators from constraints
        CalculatePropagators();
    }

    private static void Validate(int height, int width, int[,] verticalEdges, int[,] horizontalEdges)
    {
        if (height < 2 || width < 2)
        {
            throw new InvalidOperationException("Height and width should be at least 2");
        }

        if (verticalEdges.GetLength(0) != height || verticalEdges.GetLength(1) != width - 1)
        {
            throw new InvalidOperationException("VerticalEdges should be of size height * (width - 1)");
        }

        if (horizontalEdges.GetLength(0) != height - 1 || horizontalEdges.GetLength(1) != width)
        {
            throw new InvalidOperationException("HorizontalEdges should be of size (height - 1) * width");
        }
    }

    private (PieceType PieceType, int Orientation) GetPieceTypeAndOrientation(int row, int col)
    {
        var isTop = row == 0;
        var isBottom = row == _height - 1;
        var isLeft = col == 0;
        var isRight = col == _width - 1;

        if (isLeft && isTop) return (PieceType.Corner, 0);
        if (isTop && isRight) return (PieceType.Corner, 1);
        if (isRight && isBottom) return (PieceType.Corner, 2);
        if (isBottom && isLeft) return (PieceType.Corner, 3);

        if (isTop) return (PieceType.Edge, 0);
        if (isRight) return (PieceType.Edge, 1);
        if (isBottom) return (PieceType.Edge, 2);
        if (isLeft) return (PieceType.Edge, 3);

        return (PieceType.Inner, 0);
    }

    private List<int> CornerPieceDomain(int orientation)
    {
        List<int> pieces = [0, _width - 1, _width * _height - 1, _width * (_height - 1)];
        return Enumerable.Range(0, 4).Select(i => 4 * pieces[i] + ((orientation + 4 - i) % 4)).ToList();
    }

    private List<int> EdgePieceDomain(int orientation)
    {
        List<int> topEdges = Enumerable.Range(1, _width - 2).ToList();
        List<int> leftEdges = Enumerable.Range(1, _height - 2).Select(e => _width * e).ToList();
        List<int> bottomEdges = Enumerable.Range(1, _width - 2).Select(e => _width * (_height - 1) + e).ToList();
        List<int> rightEdges = Enumerable.Range(1, _height - 2).Select(e => _width * e + _width - 1).ToList();

        List<List<int>> pieces = [topEdges, rightEdges, bottomEdges, leftEdges];
        return Enumerable.Range(0, 4).SelectMany(i => pieces[i].Select(o => 4 * o + ((orientation + 4 - i) % 4))).ToList();
    }

    private List<int> InnerPieceDomain()
    {
        List<int> pieces = [];
        for (int r = 1; r < _height - 1; r++)
        {
            for (int c = 1; c < _width - 1; c++)
            {
                pieces.Add(r * _width + c);
            }
        }

        List<int> orientations = [0, 1, 2, 3];
        return pieces.SelectMany(p => orientations.Select(o => 4 * p + o)).ToList();
    }

    public new void PrintStatistics()
    {
        Console.WriteLine($"Size: {_width}x{_height}\n");

        base.PrintStatistics();
    }

    public void PrintSolution(Solution solution)
    {
        for (var row = 0; row < _height; row++)
        {
            for (var col = 0; col < _width; col++)
            {
                Console.Write($"{solution[_variables[row, col]],2} ");
            }

            Console.WriteLine();
        }
    }

    public struct InputData
    {
        public int Height;
        public int Width;
        public int[,] VerticalEdges;
        public int[,] HorizontalEdges;
    }

    public static InputData ParkerJigsaw => new()
    {
        // Source: https://www.youtube.com/watch?v=b5nElEbbnfU
        Height = 5,
        Width = 5,
        VerticalEdges = new int[,]{
            { 0,  0,  0,  5},
            {11,  2,  9,  6},
            {11,  9,  3,  5},
            { 7,  3,  5,  5},
            {13, 13, 11,  1}
        },
        HorizontalEdges = new int[,]{
            {1,  3,  9,  8, 10},
            {13, 8,  3,  5,  7},
            {13, 9,  6, 11,  0},
            {6,  4,  3,  7, 12}
        }
    };
}
