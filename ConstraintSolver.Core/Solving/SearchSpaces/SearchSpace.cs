using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class SearchSpace
{
    private readonly Store _store;

    private readonly Queue<IPropagator> _active = [];

    private readonly LinkedList<IPropagator> _atFixpoint = [];

    private readonly List<IPropagator> _subsumed = [];

    private readonly int _depth;
    public int Depth => _depth;

    public SearchSpace(Model model)
    {
        _depth = 0;
        _store = new Store(model);
        _active = new Queue<IPropagator>(model.GetPropagators());
    }

    public SearchSpace(Store store, int branchIndex, IEnumerable<IPropagator> propagators, int depth)
    {
        _depth = depth;
        _store = new Store(store);

        var branchVariableIndex = _store.Branch(branchIndex);
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
                _store.UpdatePriority(variableIndex);
            }
        }

        // No more active propagators
        return (_atFixpoint, _store);
    }

}