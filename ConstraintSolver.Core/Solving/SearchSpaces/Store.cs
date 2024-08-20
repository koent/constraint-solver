using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Collections;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Variables;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class Store
{
    private readonly List<Variable> _variables = [];

    private readonly UpdatablePriorityQueue<Variable> _branchVariables;

    public Store(Model model)
    {
        _variables = model.Variables.Select(v => new Variable(v)).ToList();
        _branchVariables = new UpdatablePriorityQueue<Variable>();
        foreach (var variable in _variables)
        {
            if (!variable.IsFixed)
            {
                _branchVariables.Enqueue(variable, variable.BranchPriority());
            }
        }
    }

    public Store(Store store)
    {
        var variableCopyMap = store._variables.ToDictionary(v => v, v => new Variable(v));
        _variables = variableCopyMap.Values.ToList();
        _branchVariables = store._branchVariables.Copy(variableCopyMap);
    }

    public bool IsSolved => _variables.All(v => v.IsFixed);

    public List<Variable> Variables => _variables;

    public Variable GetBranchVariable()
    {
        if (!_branchVariables.TryPeak(out var variable))
        {
            throw new InvalidOperationException("No open variables");
        }
        return variable;
    }

    public Variable Branch(int branchIndex)
    {
        var branchVariable = _branchVariables.Dequeue();
        branchVariable.Branch(branchIndex);
        return branchVariable;
    }

    public void UpdatePriority(Variable variable)
    {
        if (variable.IsFixed)
        {
            if (!_branchVariables.TryRemove(variable))
            {
                throw new InvalidOperationException("Variable is not enqueued");
            }
        }
        else
        {
            if (!_branchVariables.TryUpdate(variable, variable.BranchPriority()))
            {
                throw new InvalidOperationException("Variable is not enqueued");
            }
        }
    }

    public override string ToString() => string.Join(", ", _variables);
}