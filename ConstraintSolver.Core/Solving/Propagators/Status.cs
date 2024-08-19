namespace ConstraintSolver.Core.Solving.Propagators;

public enum Status
{
    Active, // Propagating can improve current store. Propagator should be scheduled. Not used since we only have a status after propagation
    AtFixpoint, // Propagating does not improve current store. Propagator should be scheduled after one of its variables domains changes
    Subsumed, // Propagating does not improve current store or any substore. Propagator should not be invoked anymore
    Failed, // Any of its variables is unsatisfiable. Propagator should not be invoked since we should backtrack
}