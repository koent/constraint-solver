using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Modeling;

public class Model
{
    public IVariable AddVariable(IVariable variable)
    {
        _variables.Add(variable);
        return variable;
    }

    public IConstraint AddConstraint(IConstraint constraint)
    {
        var variables = constraint.Variables().ToList();
        var nonExistingVariables = variables.Except(_variables).ToList();
        if (nonExistingVariables.Count > 0)
        {
            throw new InvalidOperationException($"Variables do not exist in model: {string.Join(", ", nonExistingVariables)}");
        }

        _constraints.Add(constraint);
        return constraint;
    }

    private readonly List<IVariable> _variables = [];
    public List<IVariable> Variables => _variables;

    private readonly List<IConstraint> _constraints = [];

    public IEnumerable<IPropagator> GetPropagators()
    {
        return _constraints.SelectMany(c => c.GetPropagators(_variables));
    }

    public void PrintStatistics()
    {
        Console.WriteLine($"Number of variables: {_variables.Count}");
        Console.WriteLine($"Number of constraints: {_constraints.Count}");
    }
}