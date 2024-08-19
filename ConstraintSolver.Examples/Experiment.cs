using System;
using System.Collections.Generic;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving;

namespace ConstraintSolver.Examples;

public class Experiment : Model
{
    private IVariable _a, _b, _c, _d, _e;

    public Experiment()
    {
        _a = AddVariable(new IntegerRange("a", 1, 5));
        _b = AddVariable(new IntegerRange("b", 1, 5));
        _c = AddVariable(new IntegerRange("c", 1, 5));
        _d = AddVariable(new IntegerRange("d", 1, 5));
        _e = AddVariable(new IntegerRange("e", 1, 5));

        foreach (var left in new List<IVariable> { _a, _b, _c, _d, _e })
        {
            foreach (var right in new List<IVariable> { _a, _b, _c, _d, _e })
            {
                if (left != right)
                {
                    AddConstraint(new Unequal(left, right));
                }
            }
        }
    }

    public void PrintSolution(Solution solution)
    {
        Console.WriteLine($"Solution: a = {solution[_a]}, b = {solution[_b]}, c = {solution[_c]}, d = {solution[_d]}, e = {solution[_e]}");
    }
}