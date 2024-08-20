using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class SearchSpace
{
    private readonly Store _store;
    public Store Store => _store;

    private readonly PropagatorCollection _propagators;

    private readonly int _depth;
    public int Depth => _depth;

    public SearchSpace(Model model)
    {
        _depth = 0;
        _store = new Store(model);
        _propagators = new PropagatorCollection(model);
    }

    public SearchSpace(SearchSpace parent, int branchIndex)
    {
        _depth = parent._depth + 1;
        _store = new Store(parent._store);

        var branchVariableIndex = _store.Branch(branchIndex);
        _propagators = new PropagatorCollection(parent._propagators, branchVariableIndex);
    }

    public bool PropagationFailed { get; private set; } = false;

    public void Propagate()
    {
        while (_propagators.HasActivePropagators)
        {
            var propagator = _propagators.DequeueActive();
            var (propagationStatus, modifiedVariablesIndices) = propagator.Invoke(_store);
            if (propagationStatus == Status.Failed)
            {
                PropagationFailed = true;
                return;
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
    }
}