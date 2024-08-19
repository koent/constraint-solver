using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public class Sudoku : Model
{
    private IVariable[,] _variables;

    private int _size;

    public int Size => _size;

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

        // row constraint pairs
        var unequalPairs = new HashSet<(IVariable, IVariable)>();

        for (var row = 0; row < size * size; row++)
        {
            unequalPairs.UnionWith(AllDifferentPairs(Enumerable.Range(0, size * size).Select(c => _variables[row, c]).ToList()));
        }

        // column constraint pairs
        for (var col = 0; col < size * size; col++)
        {
            unequalPairs.UnionWith(AllDifferentPairs(Enumerable.Range(0, size * size).Select(r => _variables[r, col]).ToList()));
        }

        // block constraint pairs
        for (var bCol = 0; bCol < size; bCol++)
        {
            for (var bRow = 0; bRow < size; bRow++)
            {
                for (int i = 0; i < size * size - 1; i++)
                {
                    var col1 = bCol * size + (i % size);
                    var row1 = bRow * size + (i / size);
                    for (int j = i + 1; j < size * size; j++)
                    {
                        var col2 = bCol * size + (j % size);
                        var row2 = bRow * size + (j / size);
                        unequalPairs.Add((_variables[row1, col1], _variables[row2, col2]));
                    }
                }
            }
        }

        // add constraints from pairs
        foreach (var (left, right) in unequalPairs)
        {
            AddConstraint(new Unequal(left, right));
        }
    }

    public void PrintSolution(Solution solution)
    {

        for (var row = 0; row < _size * _size; row++)
        {
            for (var col = 0; col < _size * _size; col++)
            {
                Console.Write(solution[_variables[row, col]]);
            }

            Console.WriteLine();
        }
    }

    private static IEnumerable<(IVariable, IVariable)> AllDifferentPairs(List<IVariable> variables)
    {
        for (int i = 0; i < variables.Count - 1; i++)
        {
            for (int j = i + 1; j < variables.Count; j++)
            {
                yield return (variables[i], variables[j]);
            }
        }
    }

    private const int _ = 0;

    public static (int Size, int[,] Clues) Puzzle3x3 => (3, new int[,]{
        {_,_,4,_,7,_,_,_,_},
        {2,_,_,5,8,3,_,_,9},
        {_,9,_,_,_,6,_,_,8},
        {_,_,8,_,9,_,_,5,7},
        {_,3,_,_,_,_,_,2,_},
        {7,5,_,_,2,_,3,_,_},
        {8,_,_,2,_,_,_,1,_},
        {9,_,_,1,4,5,_,_,3},
        {_,_,_,_,3,_,7,_,_}});

    public static (int Size, int[,] Clues) Puzzle2x2 => (2, new int[,]{
        {_,_,_,_},
        {_,4,_,1},
        {3,_,1,_},
        {_,_,_,_}});
}
