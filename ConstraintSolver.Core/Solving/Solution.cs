using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ConstraintSolver.Core.Modeling.Variables;

namespace ConstraintSolver.Core.Solving;

public class Solution : IReadOnlyDictionary<IVariable, int>
{
    private readonly IReadOnlyDictionary<IVariable, int> _values;

    public Solution(Store store)
    {
        if (!store.IsSolved)
        {
            throw new InvalidOperationException("Can only create solution from solved store");
        }

        _values = store.Variables.ToDictionary(v => v.GetModelVariable(), v => v.Domain().Single());
    }

    public int this[IVariable key] => _values[key];

    public IEnumerable<IVariable> Keys => _values.Keys;

    public IEnumerable<int> Values => _values.Values;

    public int Count => _values.Count;

    public bool ContainsKey(IVariable key) => _values.ContainsKey(key);

    public IEnumerator<KeyValuePair<IVariable, int>> GetEnumerator() => _values.GetEnumerator();

    public bool TryGetValue(IVariable key, [MaybeNullWhen(false)] out int value) => _values.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
}