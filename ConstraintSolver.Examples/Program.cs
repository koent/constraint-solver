using System;
using System.Linq;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public static class Program
{
    private const int _ = 0;

    public static void Main()
    {
        var tectonic = new Tectonic(Tectonic.Small20240819);

        var solver = new Solver(tectonic);

        var solutions = solver.Solve().ToList();

        // Print
        Console.WriteLine($"{solutions.Count} solution(s)");
        if (solutions.Count == 0) return;

        foreach (var solution in solutions)
        {
            Console.WriteLine();
            tectonic.PrintSolution(solution);
        }
    }
}