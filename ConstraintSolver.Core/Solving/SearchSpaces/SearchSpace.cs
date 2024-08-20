using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class SearchSpace
{
    private readonly Store _store;

    private readonly PropagatorCollection _propagators;

    private readonly int _depth;
    public int Depth => _depth;

    public SearchSpace(Model model)
    {
        _depth = 0;
        _store = new Store(model);
        _propagators = new PropagatorCollection(model.GetPropagators());
    }

    public SearchSpace(Store store, int branchIndex, IEnumerable<IPropagator> propagators, int depth)
    {
        _depth = depth;
        _store = new Store(store);
        _propagators = new PropagatorCollection([]);

        var branchVariableIndex = _store.Branch(branchIndex);
        foreach (var propagator in propagators)
        {
            if (propagator.VariableIndices().Contains(branchVariableIndex))
            {
                _propagators.EnqueueActive(propagator);
            }
            else
            {
                _propagators.AddLastAtFixpoint(propagator);
            }
        }
    }

    public (IEnumerable<IPropagator>, Store) Propagate()
    {
        while (_propagators.HasActivePropagators)
        {
            var propagator = _propagators.DequeueActive();
            var (propagationStatus, modifiedVariablesIndices) = propagator.Invoke(_store);
            if (propagationStatus == Status.Failed)
            {
                return (null, null);
            }
            var node = _propagators.FirstAtFixpoint;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.VariableIndices().Intersect(modifiedVariablesIndices).Any())
                {
                    _propagators.EnqueueActive(node.Value);
                    _propagators.RemoveAtFixpoint(node);
                }
                node = next;
            }

            if (propagationStatus == Status.AtFixpoint)
            {
                _propagators.AddLastAtFixpoint(propagator);
            }

            foreach (var variableIndex in modifiedVariablesIndices)
            {
                _store.UpdatePriority(variableIndex);
            }
        }

        // No more active propagators
        return (_propagators.AtFixpoint, _store);
    }
}