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

        var solver = new Solver(model);

        var solution = solver.Solve().FirstOrDefault();
        
        if (solution == null)
        {
            Console.WriteLine("No solution");
            return;
        }

        model.PrintSolution(solution);
    }
}