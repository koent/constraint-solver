using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    private List<IPropagator> _propagators;

    public void CalculatePropagators()
    {
        _propagators = _constraints
            .SelectMany(c => c.GetPropagators(_variables))
            .ToHashSet(new PropagatorComparer())
            .ToList();
    }

    public IEnumerable<IPropagator> GetPropagators() => _propagators;

    public void PrintStatistics()
    {
        Console.WriteLine($"Number of variables: {_variables.Count}");
        Console.WriteLine($"Number of constraints: {_constraints.Count}");
        Console.WriteLine($"Number op propagators: {_propagators.Count}");
    }

    private class PropagatorComparer : IEqualityComparer<IPropagator>
    {
        public bool Equals(IPropagator left, IPropagator right)
        {
            if (left.GetType() != right.GetType()) return false;

            var leftVariableIndices = left.VariableIndices().ToList();
            var rightVariableIndices = right.VariableIndices().ToList();

            if (leftVariableIndices.Count != rightVariableIndices.Count) return false;

            return Enumerable.Zip(leftVariableIndices, rightVariableIndices).All(pair => pair.First == pair.Second);
        }

        public int GetHashCode([DisallowNull] IPropagator obj)
        {
            return obj.VariableIndices().Aggregate(0, (h, v) => ((h << 5) + h) ^ v);
        }
    }
}