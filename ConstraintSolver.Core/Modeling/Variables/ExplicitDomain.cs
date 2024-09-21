using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Modeling.Variables;

public class ExplicitDomain(string name, IEnumerable<int> domain) : IVariable
{
    private readonly List<int> _domain = domain.ToList();

    public override string ToString() => name;

    public IEnumerable<int> Domain() => _domain;
}
