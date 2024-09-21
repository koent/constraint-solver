using System;
using System.Linq;
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

        var solver = new Solver(model);

        var nofSolutions = 0;
        foreach (var solution in solver.Solve().Take(_maxNofSolutions))
        {
            Console.WriteLine();
            Console.WriteLine($"# Solution {nofSolutions++}:\n");
            model.PrintSolution(solution);
            solution.PrintStatistics();
        }
    }
}