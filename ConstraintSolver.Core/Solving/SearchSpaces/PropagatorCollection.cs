using System.Collections.Generic;
using System.Linq;
using ConstraintSolver.Core.Modeling;
using ConstraintSolver.Core.Solving.Propagators;

namespace ConstraintSolver.Core.Solving.SearchSpaces;

public class PropagatorCollection
{
    private readonly Queue<IPropagator> _active = [];

    private readonly LinkedList<IPropagator> _atFixpoint = [];

    public PropagatorCollection(Model model)
    {
        _active = new Queue<IPropagator>(model.GetPropagators());
    }

    public PropagatorCollection(PropagatorCollection parent, int branchVariableIndex)
    {
        _atFixpoint = new LinkedList<IPropagator>(parent._atFixpoint);
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