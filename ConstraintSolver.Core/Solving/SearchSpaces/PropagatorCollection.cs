using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class PropagatorCollection
{
    private readonly Queue<IPropagator> _active = [];

    private readonly LinkedList<IPropagator> _atFixpoint = [];

    public PropagatorCollection(IEnumerable<IPropagator> activePropagators)
    {
        _active = new Queue<IPropagator>(activePropagators);
    }

    public PropagatorCollection(IEnumerable<IPropagator> atFixpointPropagators, int branchVariableIndex)
    {
        _atFixpoint = new LinkedList<IPropagator>(atFixpointPropagators);
        UpdateForModifiedVariableIndices([branchVariableIndex]);
    }

    public IEnumerable<IPropagator> AtFixpoint => _atFixpoint;

    public bool HasActivePropagators => _active.Count > 0;

    public IPropagator DequeueActive()
    {
        return _active.Dequeue();
    }

    public void AddLastAtFixpoint(IPropagator propagator)
    {
        _atFixpoint.AddLast(propagator);
    }

    public void UpdateForModifiedVariableIndices(IEnumerable<int> modifiedVariablesIndices)
    {
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
    }
}