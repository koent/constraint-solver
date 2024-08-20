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

    public Store(Store parent, int branchVariableIndex, int branchIndex)
    {
        _variables = parent._variables.Select(v => new Variable(v)).ToList();
        _branchVariablesIndices = parent._branchVariablesIndices.Copy();

        Branch(branchVariableIndex, branchIndex);
    }

    public bool IsSolved => _variables.All(v => v.IsFixed);

    public List<Variable> Variables => _variables;

    public int GetBranchVariableIndex()
    {
        if (!_branchVariablesIndices.TryPeak(out var variableIndex))
        {
            throw new InvalidOperationException("No open variables");
        }

        return variableIndex;
    }

    public void Branch(int branchVariableIndex, int branchIndex)
    {
        if (!_branchVariablesIndices.TryRemove(branchVariableIndex))
        {
            throw new InvalidOperationException("Cannot branch on this variable");
        }

        _variables[branchVariableIndex].Branch(branchIndex);
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