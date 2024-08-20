using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class PropagatorCollection
{
    private readonly Queue<IPropagator> _active = [];

    private readonly HashSet<IPropagator> _atFixpoint = [];

    private readonly List<List<IPropagator>> _propagatorPerVariableIndex;

    public PropagatorCollection(Model model)
    {
        var propagators = model.GetPropagators().ToList();
        _active = new Queue<IPropagator>(propagators);

        _propagatorPerVariableIndex = Enumerable.Repeat(0, model.Variables.Count).Select(_ => new List<IPropagator>()).ToList();
        foreach (var propagator in propagators)
        {
            foreach (var variableIndex in propagator.VariableIndices())
            {
                _propagatorPerVariableIndex[variableIndex].Add(propagator);
            }
        }
    }

    public PropagatorCollection(PropagatorCollection parent, int branchVariableIndex)
    {
        _atFixpoint = new HashSet<IPropagator>(parent._atFixpoint);
        _propagatorPerVariableIndex = parent._propagatorPerVariableIndex;
        
        UpdateForModifiedVariableIndices([branchVariableIndex]);
    }

    public bool HasActivePropagators => _active.Count > 0;

    public IPropagator DequeueActive()
    {
        return _active.Dequeue();
    }

    public void AddAtFixpoint(IPropagator propagator)
    {
        _atFixpoint.Add(propagator);
    }

    public void UpdateForModifiedVariableIndices(IEnumerable<int> modifiedVariablesIndices)
    {
        foreach (var variableIndex in modifiedVariablesIndices)
        {
            foreach (var propagator in _propagatorPerVariableIndex[variableIndex])
            {
                if (_atFixpoint.Contains(propagator))
                {
                    _active.Enqueue(propagator);
                    _atFixpoint.Remove(propagator);
                }
            }
        }
    }
}