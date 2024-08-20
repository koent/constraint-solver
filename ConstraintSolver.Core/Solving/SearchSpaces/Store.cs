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

    private readonly UpdatablePriorityQueue<int> _branchVariablesIndices;

    public Store(Model model)
    {
        _variables = model.Variables.Select(v => new Variable(v)).ToList();
        _branchVariablesIndices = new UpdatablePriorityQueue<int>();

        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            if (!variable.IsFixed)
            {
                _branchVariablesIndices.Enqueue(i, variable.BranchPriority());
            }
        }
    }

    public Store(Store store)
    {
        _variables = store._variables.Select(v => new Variable(v)).ToList();
        _branchVariablesIndices = store._branchVariablesIndices.Copy();
    }

    public bool IsSolved => _variables.All(v => v.IsFixed);

    public List<Variable> Variables => _variables;

    public Variable GetBranchVariable()
    {
        if (!_branchVariablesIndices.TryPeak(out var variableIndex))
        {
            throw new InvalidOperationException("No open variables");
        }
        return _variables[variableIndex];
    }

    public int Branch(int branchIndex)
    {
        var branchVariableIndex = _branchVariablesIndices.Dequeue();
        _variables[branchVariableIndex].Branch(branchIndex);
        return branchVariableIndex;
    }

    public void UpdatePriority(int variableIndex)
    {
        var variable = _variables[variableIndex];
        if (variable.IsFixed)
        {
            if (!_branchVariablesIndices.TryRemove(variableIndex))
            {
                throw new InvalidOperationException("Variable is not enqueued");
            }
        }
        else
        {
            if (!_branchVariablesIndices.TryUpdate(variableIndex, variable.BranchPriority()))
            {
                throw new InvalidOperationException("Variable is not enqueued");
            }
        }
    }

    public override string ToString() => string.Join(", ", _variables);
}