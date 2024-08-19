using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving;

public class SearchSpace
{
    private Store _store;

    private Queue<IPropagator> _active = [];

    private LinkedList<IPropagator> _atFixpoint = [];

    private List<IPropagator> _subsumed = [];

    private readonly int _depth;
    public int Depth => _depth;

    public SearchSpace(Model model)
    {
        _depth = 0;
        _store = new Store(model);

        foreach (var constraint in model.Constraints)
        {
            var variableIndices = model.VariableIndices(constraint).ToList();
            switch (constraint)
            {
                case Modeling.Constraints.Unequal:
                    var leftIndex = variableIndices[0];
                    var rightIndex = variableIndices[1];
                    _active.Enqueue(new Unequal(leftIndex, rightIndex));
                    break;
                case Modeling.Constraints.Constant constantConstraint:
                    var variableIndex = variableIndices[0];
                    _active.Enqueue(new Constant(variableIndex, constantConstraint.ConstantValue));
                    break;
                case Modeling.Constraints.AtMost atMostConstraint:
                    var variableIndex2 = variableIndices[0];
                    _active.Enqueue(new AtMost(variableIndex2, atMostConstraint.LimitValue));
                    break;
                default:
                    throw new InvalidOperationException($"No propagator for constraint {constraint.GetType().FullName}");
            }
        }
    }

    public SearchSpace(Store store, int branchIndex, IEnumerable<IPropagator> propagators, int depth)
    {
        _depth = depth;
        _store = new Store(store);
        // _active = new Queue<IPropagator>(propagators);
        // _store.Branch(branchIndex);

        var branchVariable = _store.Branch(branchIndex);
        var branchVariableIndex = _store.Variables.IndexOf(branchVariable);
        foreach (var propagator in propagators)
        {
            if (propagator.VariableIndices().Contains(branchVariableIndex))
            {
                _active.Enqueue(propagator);
            }
            else
            {
                _atFixpoint.AddLast(propagator);
            }
        }
    }

    public (IEnumerable<IPropagator>, Store) Propagate()
    {
        // Console.WriteLine($"Propagating search space at depth {_depth}");

        // COCP T14 page 31
        while (_active.Count > 0)
        {
            var propagator = _active.Dequeue();
            var (propagationStatus, modifiedVariablesIndices) = propagator.Invoke(_store);
            if (propagationStatus == Status.Failed)
            {
                return (_atFixpoint, null);
            }
            if (propagationStatus == Status.Subsumed)
            {
                _subsumed.Add(propagator);
            }

            var node = _atFixpoint.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.VariableIndices().Intersect(modifiedVariablesIndices).Any())
                {
                    _active.Enqueue(node.Value);
                    _atFixpoint.Remove(node);
                }
                node = next;
            }

            if (propagationStatus == Status.AtFixpoint)
            {
                _atFixpoint.AddLast(propagator);
            }

            foreach (var variableIndex in modifiedVariablesIndices)
            {
                var variable = _store.Variables[variableIndex];
                _store.UpdatePriority(variable);
            }
        }
        // No more active propagators
        return (_atFixpoint, _store);
    }

}