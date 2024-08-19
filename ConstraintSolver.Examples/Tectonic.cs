using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public class Tectonic : Model
{
    private IVariable[,] _variables;

    private int _width;

    private int _height;

    public Tectonic(InputData inputData) : this(inputData.Width, inputData.Height, inputData.MaxBoxSize, inputData.Clues, inputData.Boxes) { }

    public Tectonic(int width, int height, int maxBoxSize, int[,] clues, List<List<(int R, int C)>> boxes)
    {
        if (clues.GetLength(0) != height || clues.GetLength(1) != width)
        {
            throw new InvalidOperationException("Invalid size");
        }

        _width = width;
        _height = height;

        // create variables
        _variables = new IVariable[height, width];

        for (var col = 0; col < width; col++)
        {
            for (var row = 0; row < height; row++)
            {
                _variables[row, col] = AddVariable(new IntegerRange($"T_{row + 1}_{col + 1}", 1, maxBoxSize));
            }
        }

        // clues constraints
        for (var col = 0; col < width; col++)
        {
            for (var row = 0; row < height; row++)
            {
                var clue = clues[row, col];
                if (clue > 0)
                {
                    AddConstraint(new Constant(_variables[row, col], clue));
                }
            }
        }

        // value at most is box size constraint
        foreach (var box in boxes)
        {
            var boxSize = box.Count;
            foreach (var (row, col) in box)
            {
                if (boxSize == maxBoxSize) continue;

                if (boxSize == 1 && clues[row, col] == 0)
                {
                    AddConstraint(new Constant(_variables[row, col], 1));
                }
                else
                {
                    AddConstraint(new AtMost(_variables[row, col], boxSize));
                }
            }
        }

        // row constraint pairs
        var unequalPairs = new HashSet<(IVariable, IVariable)>();

        for (var row = 0; row < height; row++)
        {
            for (var col = 0; col < width - 1; col++)
            {
                unequalPairs.Add((_variables[row, col], _variables[row, col + 1]));
            }
        }

        // column constraint pairs
        for (var col = 0; col < width; col++)
        {
            for (var row = 0; row < height - 1; row++)
            {
                unequalPairs.Add((_variables[row, col], _variables[row + 1, col]));
            }
        }

        // diagonal constraint pairs
        for (var row = 0; row < height - 1; row++)
        {
            for (var col = 0; col < width; col++)
            {
                if (col != 0)
                {
                    unequalPairs.Add((_variables[row, col], _variables[row + 1, col - 1]));
                }

                if (col != width - 1)
                {
                    unequalPairs.Add((_variables[row, col], _variables[row + 1, col + 1]));
                }
            }
        }

        // boxes constraint pairs
        foreach (var box in boxes)
        {
            unequalPairs.UnionWith(AllDifferentPairs(box.Select(pair => _variables[pair.R, pair.C]).ToList()));
        }

        // add constraints from pairs
        foreach (var (left, right) in unequalPairs)
        {
            AddConstraint(new Unequal(left, right));
        }
    }


    public void PrintSolution(Solution solution)
    {

        for (var row = 0; row < _height; row++)
        {
            for (var col = 0; col < _width; col++)
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

    public struct InputData
    {
        public int Width;
        public int Height;
        public int MaxBoxSize;
        public int[,] Clues;
        public List<List<(int R, int C)>> Boxes;
    }

    private const int _ = 0;

    public static InputData Small20240819 => new()
    {
        Width = 7,
        Height = 6,
        MaxBoxSize = 5,
        Clues = new int[,] {
            {_,_,_,_,_,_,_},
            {_,2,_,_,_,2,_},
            {_,_,_,_,_,_,_},
            {_,4,_,_,_,_,_},
            {_,_,_,_,_,5,_},
            {_,4,_,_,_,_,_}
        },
        Boxes = [[(0, 0), (0, 1), (1, 0), (1, 1), (1, 2)], [(0, 2), (0, 3)], [(0, 4), (0, 5), (1, 3), (1, 4), (1, 5)], [(0, 6)], [(1, 6), (2, 6), (3, 5), (3, 6), (4, 6)], [(2, 0), (2, 1), (3, 0), (3, 1), (4, 1)], [(2, 2), (3, 2)], [(2, 3), (2, 4), (2, 5), (3, 3), (3, 4)], [(4, 0), (5, 0), (5, 1), (5, 2)], [(4, 2), (4, 3), (4, 4), (4, 5), (5, 5)], [(5, 3), (5, 4)], [(5, 6)]]
    };

    public static InputData Large20240717 => new()
    {
        Width = 14,
        Height = 15,
        MaxBoxSize = 6,
        Clues = new int[,] {
            {_,_,_,_,_,_,_,_,_,_,_,_,_,_},
            {_,6,_,_,3,_,_,4,_,_,4,_,_,4},
            {5,_,_,_,2,_,_,_,_,5,_,_,_,_},
            {_,2,_,_,_,_,4,_,_,_,4,_,_,5},
            {5,3,4,_,5,_,_,_,_,_,_,6,_,_},
            {_,_,_,_,_,_,_,_,5,_,_,_,_,_},
            {_,3,_,5,_,_,_,_,_,_,_,_,_,3},
            {_,_,_,_,_,_,_,_,_,_,_,_,6,_},
            {_,4,_,1,_,_,5,_,2,_,_,_,_,_},
            {_,5,_,_,_,_,_,_,4,_,_,_,_,6},
            {_,_,_,_,_,_,_,_,5,_,3,_,_,3},
            {_,_,_,_,4,_,_,_,_,_,_,4,_,_},
            {5,_,_,_,_,_,_,_,_,_,_,_,6,_},
            {_,_,_,_,_,5,_,_,6,_,6,_,_,5},
            {_,1,_,_,_,_,2,_,_,2,_,_,4,_}
        },
        Boxes = [[(0, 0), (0, 1), (1, 0), (1, 1), (2, 0), (3, 0)], [(0, 2), (0, 3), (1, 2)], [(0, 4), (1, 3), (1, 4), (2, 2), (2, 3), (2, 4)], [(0, 5), (1, 5), (1, 6), (2, 5), (2, 6), (3, 6)], [(0, 6), (0, 7), (0, 8), (1, 7), (1, 8)], [(0, 9), (0, 10), (0, 11), (1, 10), (1, 11), (2, 10)], [(0, 12)], [(0, 13), (1, 12), (1, 13), (2, 12), (2, 13), (3, 13)], [(1, 9), (2, 8), (2, 9), (3, 8), (3, 9), (3, 10)], [(2, 1), (3, 1), (3, 2), (4, 0), (4, 1), (4, 2)], [(2, 7), (3, 7), (4, 7), (5, 5), (5, 6), (5, 7)], [(2, 11), (3, 11), (3, 12), (4, 10), (4, 11), (4, 12)], [(3, 3), (3, 4), (4, 3), (4, 4), (5, 3)], [(3, 5), (4, 5), (4, 6)], [(4, 8), (4, 9), (5, 8), (5, 9), (6, 8)], [(4, 13), (5, 13)], [(5, 0), (5, 1)], [(5, 2)], [(5, 4), (6, 4), (7, 4)], [(5, 10), (6, 9), (6, 10)], [(5, 11), (5, 12), (6, 12), (6, 13), (7, 13)], [(6, 0), (6, 1), (6, 2), (6, 3), (7, 1), (7, 2)], [(6, 5), (6, 6), (7, 5), (7, 6), (8, 5), (8, 6)], [(6, 7), (7, 7), (7, 8), (8, 7), (8, 8)], [(6, 11), (7, 9), (7, 10), (7, 11), (7, 12), (8, 10)], [(7, 0), (8, 0), (8, 1), (9, 0), (9, 1), (10, 0)], [(7, 3), (8, 3), (8, 4), (9, 3)], [(8, 2), (9, 2), (10, 1), (10, 2), (10, 3), (11, 2)], [(8, 9), (9, 8), (9, 9), (9, 10), (10, 9), (10, 10)], [(8, 11), (8, 12), (8, 13), (9, 12), (9, 13), (10, 13)], [(9, 4), (9, 5), (10, 4), (10, 5), (11, 3), (11, 4)], [(9, 6), (10, 6), (10, 7), (10, 8), (11, 6)], [(9, 7)], [(9, 11), (10, 11)], [(10, 12), (11, 11), (11, 12), (11, 13), (12, 11), (12, 12)], [(11, 0), (11, 1), (12, 0), (13, 0), (14, 0)], [(11, 5), (12, 4), (12, 5), (12, 6), (13, 4), (13, 5)], [(11, 7), (11, 8), (12, 7), (13, 7)], [(11, 9), (11, 10), (12, 10), (13, 10), (13, 11), (14, 10)], [(12, 1), (12, 2), (13, 1), (13, 2), (14, 1)], [(12, 3), (13, 3), (14, 2), (14, 3)], [(12, 8), (12, 9), (13, 8), (14, 7), (14, 8), (14, 9)], [(12, 13), (13, 12), (13, 13), (14, 12), (14, 13)], [(13, 6), (14, 4), (14, 5), (14, 6)], [(13, 9)], [(14, 11)]]
    };

    public static InputData Medium20240527 => new()
    {
        Width = 10,
        Height = 10,
        MaxBoxSize = 5,
        Clues = new int[,] {
            {_,_,_,5,_,_,_,_,_,5},
            {_,_,_,_,_,_,_,_,_,_},
            {_,_,1,_,_,_,_,_,_,_},
            {_,_,_,_,_,_,_,_,_,_},
            {_,_,_,_,_,_,_,4,_,_},
            {_,_,_,_,_,_,_,_,_,4},
            {5,_,_,_,_,_,4,_,_,_},
            {_,_,_,_,_,_,_,_,_,_},
            {_,_,_,_,3,_,_,_,_,3},
            {1,5,1,2,_,_,4,_,_,_}
        },
        Boxes = [[(0, 0), (0, 1), (0, 2), (0, 3), (1, 1)], [(0, 4), (0, 5), (0, 6), (0, 7), (1, 7)], [(0, 8), (0, 9), (1, 8), (1, 9), (2, 9)], [(1, 0), (2, 0), (3, 0), (4, 0)], [(1, 2), (2, 1), (2, 2), (3, 1), (3, 2)], [(1, 3), (2, 3), (2, 4), (3, 3), (3, 4)], [(1, 4), (1, 5), (1, 6), (2, 5)], [(2, 6), (3, 5), (3, 6), (3, 7), (4, 7)], [(2, 7), (2, 8), (3, 8), (3, 9), (4, 8)], [(4, 1), (5, 1), (5, 2), (6, 1), (6, 2)], [(4, 2), (4, 3), (5, 3), (6, 3)], [(4, 4), (4, 5), (4, 6), (5, 4), (5, 5)], [(4, 9), (5, 9), (6, 8), (6, 9), (7, 9)], [(5, 0), (6, 0), (7, 0), (8, 0), (9, 0)], [(5, 6), (6, 4), (6, 5), (6, 6), (7, 6)], [(5, 7), (5, 8), (6, 7)], [(7, 1), (7, 2), (8, 1), (8, 2), (9, 1)], [(7, 3), (8, 3), (8, 4), (9, 2), (9, 3)], [(7, 4), (7, 5)], [(7, 7), (7, 8), (8, 7), (8, 8), (8, 9)], [(8, 5), (8, 6), (9, 4), (9, 5), (9, 6)], [(9, 7)], [(9, 8), (9, 9)]]
    };

    public static InputData Large20240522 => new()
    {
        Width = 11,
        Height = 11,
        MaxBoxSize = 6,
        Clues = new int[,] {
            {_,_,_,_,3,_,_,_,1,_,5},
            {4,_,_,_,2,_,_,_,_,_,_},
            {_,_,_,_,_,_,_,_,_,_,_},
            {_,_,_,_,_,2,_,_,6,5,3},
            {_,_,_,_,5,_,_,_,_,_,_},
            {_,_,_,_,_,_,_,_,_,_,4},
            {_,_,_,4,_,_,_,_,_,_,_},
            {5,_,_,_,_,6,_,_,5,_,1},
            {_,_,1,_,_,_,_,_,_,_,4},
            {_,_,_,_,_,_,_,_,_,_,_},
            {_,2,_,_,_,_,5,_,_,_,4}
        },
        Boxes = [[(0, 0), (1, 0), (2, 0), (2, 1)], [(0, 1), (0, 2), (0, 3), (0, 4), (0, 5), (1, 4)], [(0, 6), (0, 7), (0, 8), (0, 9), (0, 10), (1, 9)], [(1, 1), (1, 2), (1, 3), (2, 2), (2, 3)], [(1, 5), (1, 6), (1, 7), (2, 6), (2, 7)], [(1, 8), (2, 8), (2, 9), (3, 7), (3, 8), (4, 8)], [(1, 10), (2, 10), (3, 9), (3, 10), (4, 10), (5, 10)], [(2, 4), (2, 5), (3, 3), (3, 4), (3, 5), (3, 6)], [(3, 0), (3, 1), (3, 2), (4, 1)], [(4, 0), (5, 0)], [(4, 2), (5, 2), (5, 3)], [(4, 3), (4, 4), (5, 4), (5, 5), (6, 4), (6, 5)], [(4, 5), (4, 6), (4, 7), (5, 6), (5, 7), (6, 6)], [(4, 9), (5, 9)], [(5, 1), (6, 0), (6, 1), (6, 2), (6, 3), (7, 0)], [(5, 8), (6, 7), (6, 8), (7, 6), (7, 7), (7, 8)], [(6, 9), (6, 10), (7, 9), (7, 10), (8, 10)], [(7, 1), (8, 0), (8, 1), (8, 2), (8, 3)], [(7, 2), (7, 3), (7, 4), (7, 5), (8, 4), (8, 5)], [(8, 6), (8, 7)], [(8, 8), (8, 9), (9, 9)], [(9, 0), (9, 1), (9, 2), (9, 3), (10, 0), (10, 1)], [(9, 4), (9, 5), (9, 6), (10, 5), (10, 6)], [(9, 7), (9, 8), (10, 7), (10, 8), (10, 9), (10, 10)], [(9, 10)], [(10, 2), (10, 3), (10, 4)]]
    };
}