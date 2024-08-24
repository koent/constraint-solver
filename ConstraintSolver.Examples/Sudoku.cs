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
    private const string SYMBOLS = " 123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

    public void PrintSolution(Solution solution, bool letters = true)
    {
        for (var row = 0; row < _size * _size; row++)
        {
            for (var col = 0; col < _size * _size; col++)
            {
                if (letters)
                {
                    Console.Write(SYMBOLS[solution[_variables[row, col]]]);
                }
                else
                {
                    Console.Write($"{solution[_variables[row, col]],2} ");
                }
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

    public static InputData Puzzle4x4Unreasonable => new()
    {
        Size = 4,
        Clues = new int[,]{
            {16,5,_,_,_,_,_,_,_,_,_,3,_,9,14,_},
            {_,14,9,13,7,_,15,16,11,10,_,_,_,3,_,_},
            {_,_,2,_,_,14,5,_,12,_,4,_,_,_,10,8},
            {_,1,_,_,_,_,_,_,_,_,5,6,2,_,16,_},
            {5,_,_,_,_,_,_,2,_,_,_,_,_,11,_,_},
            {11,13,_,_,_,_,9,_,3,_,_,2,6,14,12,_},
            {9,_,3,10,14,6,_,_,1,_,_,16,5,_,_,_},
            {_,_,_,_,_,16,_,4,6,_,_,_,_,10,_,3},
            {13,_,11,_,_,_,_,12,15,_,16,_,_,_,_,_},
            {_,_,_,3,8,_,_,11,_,_,13,12,16,2,_,7},
            {_,16,8,6,2,_,_,9,_,7,_,_,_,_,13,1},
            {_,_,15,_,_,_,_,_,9,_,_,_,_,_,_,5},
            {_,8,_,15,3,10,_,_,_,_,_,_,_,_,4,_},
            {3,10,_,_,_,5,_,8,_,14,11,_,_,1,_,_},
            {_,_,4,_,_,_,2,15,16,1,_,13,3,5,7,_},
            {_,7,6,_,4,_,_,_,_,_,_,_,_,_,9,2}
        }
    };

    public static InputData Puzzle5x5Basic => new()
    {
        Size = 5,
        Clues = new int[,]{
            {_,14,_,25,_,_,_,10,_,_,22,12,_,6,18,24,15,_,_,7,_,16,_,19,_},
            {_,_,18,_,5,_,_,1,14,20,_,_,16,_,7,_,6,3,_,11,_,_,22,4,9},
            {_,_,17,_,_,19,6,_,9,_,1,24,20,21,14,_,_,_,_,13,_,_,3,_,11},
            {20,19,_,7,24,_,15,_,_,16,13,_,_,_,_,_,_,23,12,1,_,_,14,_,_},
            {4,9,_,_,_,_,_,18,_,22,_,_,_,_,11,_,14,16,_,25,23,1,_,12,_},
            {3,_,_,_,23,17,_,21,19,6,_,_,_,_,_,12,9,_,13,_,_,_,11,_,_},
            {10,8,_,_,20,_,24,_,_,_,6,18,_,_,_,7,23,19,_,_,_,_,12,_,14},
            {_,_,12,_,_,_,_,2,3,10,24,_,_,9,19,20,_,18,6,_,_,17,4,_,_},
            {6,_,_,16,14,_,_,_,_,7,_,17,_,25,2,_,24,1,_,_,_,_,23,21,19},
            {_,_,_,_,19,16,_,9,_,_,23,3,21,_,_,10,11,_,_,22,_,_,_,_,6},
            {12,_,14,11,4,8,10,_,2,_,_,_,17,1,21,_,_,_,_,_,20,7,_,6,_},
            {_,_,_,_,_,7,22,_,_,15,2,_,6,_,4,14,20,_,_,9,5,21,_,_,17},
            {21,5,22,17,_,_,_,4,_,_,_,16,_,3,_,_,_,2,_,_,_,18,10,24,25},
            {16,_,_,6,9,20,_,_,12,25,7,_,22,_,10,3,_,_,8,5,_,_,_,_,_},
            {_,20,_,2,7,_,_,_,_,_,18,19,25,_,_,_,17,_,15,4,3,14,8,_,12},
            {5,_,_,_,_,22,_,_,18,21,_,_,2,8,24,_,_,7,_,17,11,_,_,_,_},
            {1,17,16,_,_,_,_,3,11,_,10,22,_,13,_,8,_,_,_,_,18,23,_,_,15},
            {_,_,3,20,_,_,4,14,_,1,19,11,_,_,25,9,13,21,_,_,_,_,5,_,_},
            {8,_,2,_,_,_,_,24,7,23,_,_,_,14,6,_,_,_,22,_,19,_,_,20,1},
            {_,_,24,_,_,_,19,_,13,8,_,_,_,_,_,25,3,15,_,10,21,_,_,_,16},
            {_,4,_,18,17,14,_,7,22,_,15,_,_,_,_,23,_,12,_,_,_,_,_,9,3},
            {_,_,8,_,_,10,2,20,_,_,_,_,_,_,9,15,_,_,11,_,16,19,_,5,24},
            {9,_,20,_,_,3,_,_,_,_,11,8,18,19,23,_,22,_,10,6,_,_,2,_,_},
            {24,3,11,_,_,9,_,15,16,_,21,_,14,_,_,5,1,13,_,_,8,_,20,_,_},
            {_,7,_,15,_,6,_,_,21,17,3,25,_,10,20,_,_,8,_,_,_,11,_,18,_}
        }
    };

    public static InputData Puzzle7x7 => new()
    {
        // Source: https://sudokugeant.cabanova.com/files/downloads/gs08-49.pdf
        Size = 7,
        Clues = new int[,]{
            { _, _, _, _, _,15,23,24,41, 3,10,35,13, 2, 7, _, _,12,48, 4, _,44,11,37, 8,39,46,49, _,28, 9,22, _, _,21,16,43,40,47,26,45,31,34,38, _, _, _, _, _},
            { _, _, _, _,47, _,46,30, _, 5, 9,20, _,19,14, _,16,21,31, _,13,41, 7,32, 6,26,22,38,23, _,29,43,45, _,33,25, _,48,15, 8, _,42,28, _,39, _, _, _, _},
            { _, _, _, _, _,42, _, _, 8,12,29,27,32, _,11, 6,10,46, _,41,19,16,43,18, _,30, 4,21, 7,48, _,20, 2,15,24, _,38,39,34,36,35, _, _, 9, _, _, _, _, _},
            { _, _, _, _, _, _, _, _, _, _, _, _,18,17,47, 5,36, _, 3,23, 8, 1,12, _, _, _,19,34,32,13,38, _,41,31,40,49,46, _, _, _, _, _, _, _, _, _, _, _, _},
            { _,38, _, _, _, _, _, _, _, _, _, 7, _,36,18,28, _,22,34,20,17, 9, _, _, _, _, _,15,35,44,11, 3, _, 6,19,37, _,21, _, _, _, _, _, _, _, _, _,30, _},
            {21, _,19, _, _, _, _, _,39,34, _, _,42, 6,43, _,37,45,44,15,33, _, _, _,20, _, _, _, 8,14, 1,49, 4, _,26,10,32, _, _,29,30, _, _, _, _, _,46, _,12},
            {18,41, _, _, _, _, _, _, _, _, _,45,49,14, _, 2,39,25,38,35, _, _, _,28, 3,27, _, _, _,16,46,30,47,34, _,20,19, 9, _, _, _, _, _, _, _, _, _, 4, 6},
            {26,30, _, _, _, _, _, _, _, _,27, 2,33, _,38,36, 6,11,25, _, _, _,39, 4,40,23, 8, _, _, _,15,45,14,48,34, _,21,19,28, _, _, _, _, _, _, _, _,41,10},
            { 6, _, 2, _, _,23, _, _, _,30,13, 4, _,49,34,20,43,15, _, _, _,45,28,44,32,36,11,35, _, _, _, 5, 9,19,46,26, _,29,22,48, _, _, _,18, _, _,14, _, 7},
            {13,47,14, _, _, 5, _, _,31,48, 1, _, 8,11,19,17,22, _, _, _,27,24,34,46, 2,10, 3, 9, 6, _, _, _,28,26,38,41,42, _,30,20, 4, _, _,44, _, _,23,37,39},
            { 4,46,12, _, _, _, _,14,24,29, _,28,16, 3,31,18, _, _, _,32,23,43,30,21, _,22,37,19,42,35, _, _, _,10,47,17, 2,13, _, 6,39,38, _, _, _, _,27,11, 8},
            {35,21,32, _,29, _,27, 5,23, _,15,22,38,10,37, _, _, _, _,45,46,25,31, _, _, _,41,14, 4,11, _, _, _, _, 1,43,49, 7,36, _,40, 9,19, _,17, _,34, 3,48},
            {17, _, 8,19, _,49,11,41, _,39,44,12,26, 9, _, _,40, _, _, _, 2,18, _, _, 5, _, _,48,31, _, _, _,37, _, _,24, 3,45,10,34, _,33,32,47, _,36, 6, _, 4},
            {10,18, _,41,45,28,48, _,34,36,19,40,20, _, _,44,26, 4, _, _, _, _, _,49, 7,33, _, _, _, _, _, 2,16,13, _, _,14, 5,31,37,15, _,21,46,30,43, _,22,35},
            { 7, 2,11,36,41,13, _,48, 6,15,20,33, _, _,17,19,45,16, 8, _, _, _,23,22, 9,28,38, _, _, _, 5,32,12,43,25, _, _,49,40,44,34,30, _,21,18,10,35,24,14},
            { _, _,21,14,34, _,33,10,12,35,18, _, _,26,46,11,20,32,40, 3, _,37,27, 7, 4,44,24,36, _, 1,45, 6,22,47,17, 5, _, _,29,15,41, 2,16, _, 8,28,39, _, _},
            { _,25,37,45, _,17,20, 2,11,14, _, _,31,16,10,22, 7,43,36,47,29,46, 5, 6,33,19,34, 8,18,15,23,26,44,28,41, 9,13, _, _,35,21,27,48,40, _, 1,12,42, _},
            {47, 3, 6, _,28,12,15,44,37, _, _, _, _,24, 5,35,14,31,13,30, _,48,42,45,43,40,18,39, _,29,16,38,19,27, 9,32, _, _, _, _, 1,36, 2,34,22, _,17, 7,41},
            {46,19, _,31,38,27,10,13, _, _, _, _, _, _,48,24,33, 6,12, _, _, _,47,29,35, 1,16, _, _, _,14,37, 7,42,39, _, _, _, _, _, _,17, 9,45,11, 5, _,49,30},
            {40, _,16,22, 5, 8,32, _, _, _,41, 3, _, _, _,23, 4,39, _, _, _, _, _,30,17,15, _, _, _, _, _,24,11,20, _, _, _,37,26, _, _, _,13,36,43,47,44, _,29},
            { _,39,29,18,23,35, _, _, _,38,42,47,17, _, _, _,15, _, _, _, _, _, _, _,13, _, _, _, _, _, _, _, 8, _, _, _,45,16,46,24, _, _, _,20,33,37,26,31, _},
            { 1,13,47,29,11, _, _, _, 2,20,48,37,12, _, _,32,44, 5, _, _, _, _, _,24,27,45, _, _, _, _, _,34,23,40, _, _,39,10,42,19,26, _, _, _,41,31,43,36, 9},
            {39, 6, 7,38, _, _, _,47,21, 4,32,49, _, _,33,15,23, 2,27, _, _, _,26,17,31,35,25, _, _, _,28,14, 5,41,11, _, _,12,16,43,48,40, _, _, _,19,10,18,24},
            {27,10,23, _, _, _,30, 7,18,45,31, _, _,15,24, 8, 3,13,29,38, _,32,21,36,22,48,40,44, _,33,12,42,43,25, 2, 1, _, _, 5,49, 9,41,20, _, _, _,28,39,37},
            {31,45, _, _, _, 2,41, 3,42,26, _, _,46,33, 4,12,30, 7,22,16,39,15,10, 1,23,38,43,28,36,19,24,27,35, 8,18,29,11, _, _,32,37,20,44,49, _, _, _,14,21},
            {49, 5,40, _, _, _,43,35,22,41,25, _, _,23,28,10,34,47,19,36, _,11,16, 2,14,13, 9,37, _,39,44,21,29, 4, 7,31, _, _,45,38, 8, 3,27, _, _, _,33,48, 1},
            {19,34,22,25, _, _, _,36,28,27,14, 9, _, _,49,43,17, 1,41, _, _, _,46, 3,12, 5,39, _, _, _,13,15, 6,37,16, _, _,24,21,23, 2, 7, _, _, _,30, 4,45,26},
            {24,14,33,44,26, _, _, _,10,43,38,16, 5, _, _,21,18,42, _, _, _, _, _, 8,19,34, _, _, _, _, _,48,32, 1, _, _,27,22,35,17,28, _, _, _,12,15,13, 2,25},
            { _,16,38, 4,31,47, _, _, _, 7,23,24, 3, _, _, _, 9, _, _, _, _, _, _, _,46, _, _, _, _, _, _, _,10, _, _, _, 6,25,20,33, _, _, _,12, 1,44,41,32, _},
            { 8, _,27,35,30,40,49, _, _, _,37,36, _, _, _,13,28,20, _, _, _, _, _,10,18,17, _, _, _, _, _,31,38, 9, _, _, _,46,43, _, _, _,11,23,26,22,25, _,19},
            {48,37, _, 5,32,19,21,49, _, _, _, _, _, _,15, 4,41,44,17, _, _, _,40,47,28,16,31, _, _, _,34,18,25,23,14, _, _, _, _, _, _,13,35, 7,46,24, _,29,42},
            {44,36,45, _,14,33,24,46,17, _, _, _, _, 5,22,38,47,49,10,25, _,34, 4,39,29,43,20,26, _,37,42, 7,30,12, 6,19, _, _, _, _,11,23,18,28,21, _,16,40,13},
            { _,28,25, 9, _,39,29,42,20,13, _, _,30,18,27,31, 8,19, 6,33, 7,38,48,14,41,32,12,24,16,21,43,46, 1,11,44,35,40, _, _, 5,22,49,17,10, _, 4,36,47, _},
            { _, _,46,17,10, _,13,16, 4,47,28, _, _,44,12,34,11,23,32,48, _, 7,22,42,25, 9,49,30, _, 5,36,19,40,33, 3,21, _, _,37, 1,18,39,15, _,27,38,20, _, _},
            {41,12, 3,11,43,18, _,19,38,31,26,25, _, _,40,29,46,37,14, _, _, _,36,27,44, 8,23, _, _, _,32,28,20,35,13, _, _,17, 4,47, 7,10, _, 2, 9,39, 5, 6,34},
            {37,35, _,49,21,14,19, _,27,17,33,48,39, _, _, 3,25,40, _, _, _, _, _,15,16,18, _, _, _, _, _, 8,42,32, _, _,34,36,24,22,29, _,46, 1,23, 6, _,12,11},
            {33, _,18, 2, _,30,34, 9, _,21, 3,41,28, 4, _, _,35, _, _, _,12,42, _, _,48, _, _,27, 5, _, _, _,13, _, _, 7,26, 1,44,46, _,37,39,24, _,29,31, _,36},
            {15,20,43, _,36, _, 6,40,14, _,47,26,37,32,44, _, _, _, _,19,30,22,38, _, _, _, 7,29,28,17, _, _, _, _,45,12,16,42, 8, _,25,35, 3, _,49, _,48,13,18},
            {22, 4,17, _, _, _, _,20,25, 2, _, 6, 7,38,32,37, _, _, _,34,21,47, 9,19, _,49,26,46,15,36, _, _, _,14,27,45,41,23, _,39,31,18, _, _, _, _, 8,16,40},
            {38,44,26, _, _, 7, _, _,13,24, 8, _,22,31,36,39, 2, _, _, _,43,35,45,12,21, 6,17, 3,19, _, _, _,34,46,37,11,48, _, 9,40,49, _, _,25, _, _,32,20, 5},
            { 9, _,48, _, _,41, _, _, _,18,16, 5, _,29, 6,46,13,10, _, _, _, 8,37,33,34,24,36,23, _, _, _,40, 3,44,49, 2, _,32,19,14, _, _, _,30, _, _,21, _,17},
            {45,32, _, _, _, _, _, _, _, _,34,10,36, _,29,42,38,26, 1, _, _, _,14, 5,39, 2,30, _, _, _,18, 4,31, 7,23, _,20,33,17, _, _, _, _, _, _, _, _, 9,22},
            {16,43, _, _, _, _, _, _, _, _, _,18,27,20, _,25,12,38,42,29, _, _, _, 9,30,46, _, _, _,34,40,36,15,24, _,33,35,44, _, _, _, _, _, _, _, _, _,21,23},
            {36, _,34, _, _, _, _, _,35,28, _, _,14,13,39, _,19,30,15,24,18, _, _, _,42, _, _, _, 1,25,27,33,21, _,12,22,37, _, _,41,43, _, _, _, _, _, 9, _,46},
            { _,40, _, _, _, _, _, _, _, _, _,38, _,37,26,33, _, 3,23,17, 4,13, _, _, _, _, _,12,44,41,10,35, _,29,32,18, _, 8, _, _, _, _, _, _, _, _, _,34, _},
            { _, _, _, _, _, _, _, _, _, _, _, _,10,22, 8,40,21, _,49,44,11,19,35, _, _, _,45,33, 2,42,30, _,46, 3, 5,15,36, _, _, _, _, _, _, _, _, _, _, _, _},
            { _, _, _, _, _, 4, _, _,46,49,12,15,48, _,16, 7,32,35, _, 6,10,31,24,34, _,11,27,18,37,23, _,47,26,17,28, _, 1,38,14,45, 3, _, _,39, _, _, _, _, _},
            { _, _, _, _,19, _,39,34, _,42, 7,23, _,47,20, _,31, 9,37, _,22,10, 8,41,49,29,14,16,43, _, 6,13,48, _, 4,46, _, 2,12,21, _,25,36, _,24, _, _, _, _},
            { _, _, _, _, _,10,18,26, 3,33,24, 1,19, 8, 2, _, _,36,46,14, _,17,44,25,37,21, 5,43, _,20, 7, 9, _, _,31, 4,23,47,49,30,13,34,12,48, _, _, _, _, _}
        }
    };
}
