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

        var branchVariableIndex = _store.Branch(branchIndex);
        _propagators = new PropagatorCollection(propagators, branchVariableIndex);
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

            _propagators.UpdateForModifiedVariableIndices(modifiedVariablesIndices);

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