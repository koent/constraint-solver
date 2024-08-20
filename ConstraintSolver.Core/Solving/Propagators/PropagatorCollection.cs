using System.Collections.Generic;

namespace ConstraintSolver.Core.Solving.Propagators;

public class PropagatorCollection
{
    private readonly Queue<IPropagator> _active = [];

    private readonly LinkedList<IPropagator> _atFixpoint = [];

    public PropagatorCollection(IEnumerable<IPropagator> propagators)
    {
        _active = new Queue<IPropagator>(propagators);
    }

    public IEnumerable<IPropagator> AtFixpoint => _atFixpoint;

    public bool HasActivePropagators => _active.Count > 0;

    public IPropagator DequeueActive()
    {
        return _active.Dequeue();
    }

    public void EnqueueActive(IPropagator propagator)
    {
        _active.Enqueue(propagator);
    }

    public void AddLastAtFixpoint(IPropagator propagator)
    {
        _atFixpoint.AddLast(propagator);
    }

    public void RemoveAtFixpoint(LinkedListNode<IPropagator> node)
    {
        _atFixpoint.Remove(node);
    }

    public LinkedListNode<IPropagator> FirstAtFixpoint => _atFixpoint.First;
}