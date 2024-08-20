using System;
using System.Linq;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public static class Program
{
    private const int _ = 0;

    public static void Main()
    {
        // var model = new Tectonic(Tectonic.Large20240522);

        var model = new Sudoku(Sudoku.Puzzle3x3Unreasonable);

        model.PrintStatistics();

        var solver = new Solver(model);

        var nofSolutions = 0;
        foreach (var solution in solver.Solve())
        {
            Console.WriteLine();
            Console.WriteLine($"Solution {nofSolutions++}:");
            model.PrintSolution(solution);
            solution.PrintStatistics();
        }
    }
}