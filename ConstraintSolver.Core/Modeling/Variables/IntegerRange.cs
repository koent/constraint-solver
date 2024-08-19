using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Modeling.Variables;

public class IntegerRange : IVariable
{
    public IntegerRange(string name, int lowerBound, int upperBound)
    {
        _name = name;
        _lowerBound = lowerBound;
        _upperBound = upperBound;
    }

    private string _name;

    private int _lowerBound;

    private int _upperBound;

    public override string ToString() => _name;

    public IEnumerable<int> Domain()
    {
        return Enumerable.Range(_lowerBound, _upperBound + _lowerBound - 1);
    }
}