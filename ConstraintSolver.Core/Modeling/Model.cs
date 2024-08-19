using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;

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
        _variableIndicesPerConstraint.Add(constraint.Variables().Select(v => _variables.IndexOf(v)));

        return constraint;
    }

    private readonly List<IEnumerable<int>> _variableIndicesPerConstraint = [];

    private readonly List<IVariable> _variables = [];
    public List<IVariable> Variables => _variables;

    private readonly List<IConstraint> _constraints = [];
    public List<IConstraint> Constraints => _constraints;

    public IEnumerable<int> VariableIndices(IConstraint constraint)
    {
        return constraint.Variables().Select(v => _variables.IndexOf(v));
    }
}