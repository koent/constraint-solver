using System.Collections.Generic;
using System.Linq;
using ModelVariable = ConstraintSolver.Core.Modeling.Variables.IVariable;

namespace ConstraintSolver.Core.Solving.Variables;

public class Variable
{
    private ModelVariable _modelVariable;

    public Variable(Variable variable)
    {
        _modelVariable = variable._modelVariable;
        _domain = variable.Domain().ToList();
    }

    public Variable(ModelVariable modelVariable)
    {
        _modelVariable = modelVariable;
        _domain = _modelVariable.Domain().ToList();
    }

    private List<int> _domain = [];

    public int BranchPriority() => _domain.Count;

    public bool IsUnsatisfiable => _domain.Count == 0;

    public bool IsFixed => _domain.Count == 1;

    public bool TryGetFixedValue(out int value)
    {
        if (_domain.Count == 1)
        {
            value = _domain.Single();
            return true;
        }

        value = default;
        return false;
    }

    public IEnumerable<int> Domain() => _domain;

    public bool Update(IEnumerable<int> newDomain)
    {
        var newDomainList = newDomain.ToList();
        var updated = newDomainList.Count < _domain.Count;
        _domain = newDomainList;
        return updated;
    }

    public bool RemoveFromDomain(int value)
    {
        if (!_domain.Contains(value))
        {
            return false;
        }

        _domain = _domain.Where(v => v != value).ToList();
        return true;
    }

    public ModelVariable GetModelVariable() => _modelVariable;

    public IEnumerable<int> BranchIndices() => _domain;

    public void Branch(int branchIndex) => Update([branchIndex]);

    public override string ToString() => $"{_modelVariable}: [{string.Join(", ", _domain)}]";
}