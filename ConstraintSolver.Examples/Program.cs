using System;
using ConstraintSolver.Core.Solving;
using ConstraintSolver.Examples.AmbiguousJigsaws;

namespace ConstraintSolver.Examples;

public static class Program
{
    private const int _ = 0;

    private const int _maxNofSolutions = 10;

    public static void Main()
    {
        // var model = new Tectonic(Tectonic.Large20240717);

        // var model = new Sudoku(Sudoku.Puzzle7x7);

        var model = new AmbiguousJigsaw(AmbiguousJigsaw.ParkerJigsaw);

        model.PrintStatistics();
        Console.WriteLine();

        var solver = new Solver(model, _maxNofSolutions);

        foreach (var solution in solver.Solve())
        {
            Console.WriteLine($"# Solution {solution.Statistics.SolutionIndex}:\n");
            model.PrintSolution(solution);
            solution.PrintStatistics();
            Console.WriteLine();
        }

        solver.PrintStatistics();
    }
}