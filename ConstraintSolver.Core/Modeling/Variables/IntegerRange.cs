using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Modeling.Variables;

public class IntegerRange(string name, int lowerBound, int upperBound) : IVariable
{
    public override string ToString() => name;

    public IEnumerable<int> Domain()
    {
        return Enumerable.Range(lowerBound, upperBound + lowerBound - 1);
    }
}