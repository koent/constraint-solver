using System;
using System.Data;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public class Sudoku : Model
{
    private const string SYMBOLS = " 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private IVariable[,] _variables;

    private int _size;

    public int Size => _size;

    public Sudoku(InputData inputData) : this(inputData.Size, inputData.Clues) { }

    public Sudoku(int size, int[,] clues)
    {
        if (clues.GetLength(0) != size * size || clues.GetLength(1) != size * size)
        {
            throw new InvalidOperationException("Invalid array size");
        }

        _size = size;

        // create variables
        _variables = new IVariable[size * size, size * size];

        for (var col = 0; col < size * size; col++)
        {
            for (var row = 0; row < size * size; row++)
            {
                _variables[row, col] = AddVariable(new IntegerRange($"S_{row + 1}_{col + 1}", 1, size * size));
            }
        }

        // clues constraints
        for (var col = 0; col < size * size; col++)
        {
            for (var row = 0; row < size * size; row++)
            {
                var clue = clues[row, col];
                if (clue > 0)
                {
                    AddConstraint(new Constant(_variables[row, col], clue));
                }
            }
        }

        // row constraints
        for (var row = 0; row < size * size; row++)
        {
            AddConstraint(new Permutation(Enumerable.Range(0, size * size).Select(c => _variables[row, c])));
        }

        // column constraints
        for (var col = 0; col < size * size; col++)
        {
            AddConstraint(new Permutation(Enumerable.Range(0, size * size).Select(r => _variables[r, col])));
        }

        // block constraints
        for (var bCol = 0; bCol < size; bCol++)
        {
            for (var bRow = 0; bRow < size; bRow++)
            {
                AddConstraint(new Permutation(Enumerable.Range(0, size * size).Select(i => _variables[bRow * size + (i / size), bCol * size + (i % size)])));
            }
        }

        // Calculate propagators from constraints
        CalculatePropagators();
    }

    public new void PrintStatistics()
    {
        Console.WriteLine($"Size: {_size * _size}x{_size * _size}\n");

        base.PrintStatistics();
    }

    public void PrintSolution(Solution solution)
    {
        for (var row = 0; row < _size * _size; row++)
        {
            for (var col = 0; col < _size * _size; col++)
            {
                Console.Write(SYMBOLS[solution[_variables[row, col]]]);
            }

            Console.WriteLine();
        }
    }

    public struct InputData
    {
        public int Size;
        public int[,] Clues;
    }

    private const int _ = 0;

    public static InputData Puzzle3x3 => new()
    {
        Size = 3,
        Clues = new int[,]{
            {_,_,4,_,7,_,_,_,_},
            {2,_,_,5,8,3,_,_,9},
            {_,9,_,_,_,6,_,_,8},
            {_,_,8,_,9,_,_,5,7},
            {_,3,_,_,_,_,_,2,_},
            {7,5,_,_,2,_,3,_,_},
            {8,_,_,2,_,_,_,1,_},
            {9,_,_,1,4,5,_,_,3},
            {_,_,_,_,3,_,7,_,_}
        }
    };

    public static InputData Puzzle3x3Unreasonable => new()
    {
        Size = 3,
        Clues = new int[,]{
            {_,6,2,_,8,_,7,_,_},
            {9,_,_,7,_,_,_,_,6},
            {4,_,_,_,_,_,_,8,_},
            {3,_,_,_,4,8,_,_,_},
            {_,_,7,_,_,_,6,_,_},
            {_,_,_,9,7,_,_,_,2},
            {_,4,_,_,_,_,_,_,3},
            {1,_,_,_,_,2,_,_,7},
            {_,_,3,_,6,_,4,9,_}
        }
    };

    public static InputData Puzzle2x2 => new()
    {
        Size = 2,
        Clues = new int[,]{
            {_,_,_,_},
            {_,4,_,1},
            {3,_,1,_},
            {_,_,_,_}
        }
    };

    public static InputData Puzzle4x4Basic => new()
    {
        Size = 4,
        Clues = new int[,]{
            {12,_,4,_,_,9,_,10,_,_,15,1,_,_,_,_},
            {10,_,_,_,_,_,_,7,_,14,_,9,4,3,_,_},
            {14,_,_,3,15,6,_,_,_,10,12,_,9,_,_,11},
            {15,_,7,_,2,4,12,_,_,8,_,_,16,_,_,_},
            {4,15,_,_,7,_,_,_,_,1,11,_,14,_,_,_},
            {_,1,5,_,3,_,_,16,_,_,7,_,_,_,_,15},
            {_,_,_,_,_,13,1,_,4,_,_,2,_,_,9,8},
            {3,_,_,2,_,12,_,_,_,_,_,8,_,_,5,_},
            {_,9,_,_,6,_,_,_,_,_,10,_,11,_,_,5},
            {5,6,_,_,1,_,_,13,_,3,8,_,_,_,_,_},
            {11,_,_,_,_,8,_,_,15,_,_,7,_,2,1,_},
            {_,_,_,1,_,16,7,_,_,_,_,5,_,_,4,6},
            {_,_,_,6,_,_,14,_,_,2,16,3,_,1,_,13},
            {1,_,_,7,_,2,5,_,_,_,9,6,8,_,_,10},
            {_,_,3,5,4,_,6,_,10,_,_,_,_,_,_,16},
            {_,_,_,_,13,11,_,_,12,_,1,_,_,6,_,4}
        }
    };
}
