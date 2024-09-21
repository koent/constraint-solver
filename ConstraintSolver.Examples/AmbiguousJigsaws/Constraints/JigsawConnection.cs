using System.Collections.Generic;
using ConstraintSolver.Core.Modeling.Constraints;
using ConstraintSolver.Core.Modeling.Variables;
using ConstraintSolver.Core.Solving.Propagators;
using ConstraintSolver.Examples.AmbiguousJigsaws.Propagators;

namespace ConstraintSolver.Examples.AmbiguousJigsaws.Constraints;

public class JigsawConnection(IVariable left, IVariable right, bool vertical, int[,] _board) : IConstraint
{
    public IEnumerable<IPropagator> GetPropagators(List<IVariable> variablesList)
    {
        yield return vertical
         ? new VerticalJigsawConnection(variablesList.IndexOf(left), variablesList.IndexOf(right), _board)
         : new HorizontalJigsawConnection(variablesList.IndexOf(left), variablesList.IndexOf(right), _board);
    }

    public IEnumerable<IVariable> Variables()
    {
        yield return left;
        yield return right;
    }
}