using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintSolver.Core.Collections;

// Based on https://github.com/eiriktsarpalis/pq-tests/blob/master/PriorityQueue/PrioritySet.cs

public class UpdatablePriorityQueue<T>
{
    private const int DefaultCapacity = 4;

    private readonly Dictionary<T, int> _index;

    private HeapEntry[] _heap;

    private int _count = 0;

    public UpdatablePriorityQueue()
    {
        _heap = [];
        _index = [];
    }

    private UpdatablePriorityQueue(HeapEntry[] heap, Dictionary<T, int> index, int count)
    {
        _count = count;
        _heap = heap;
        _index = index;
    }

    public int Count => _count;

    public UpdatablePriorityQueue<T> Copy()
    {
        return new UpdatablePriorityQueue<T>(
            _heap.Select(e => new HeapEntry { Element = e.Element, Priority = e.Priority }).ToArray(),
            _index.ToDictionary(),
            _count
        );
    }

    public bool TryPeak(out T element)
    {
        if (_count == 0)
        {
            element = default;
            return false;
        }

        element = _heap[0].Element;
        return true;
    }

    public void Enqueue(T element, int priority)
    {
        if (_index.ContainsKey(element))
        {
            throw new InvalidOperationException("Duplicate element");
        }

        Insert(element, priority);
    }

    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        RemoveIndex(0, out T result, out int _);
        return result;
    }

    public bool TryUpdate(T element, int priority)
    {
        if (!_index.TryGetValue(element, out int index))
        {
            return false;
        }

        UpdateIndex(index, priority);
        return true;
    }

    public bool TryRemove(T element)
    {
        if (!_index.TryGetValue(element, out int index))
        {
            return false;
        }

        RemoveIndex(index, out var _, out var _);
        return true;
    }

    private void Insert(in T element, in int priority)
    {
        if (_count == _heap.Length)
        {
            Resize(ref _heap);
        }

        SiftUp(_count++, in element, in priority);
    }

    private void RemoveIndex(int index, out T element, out int priority)
    {
        (element, priority) = _heap[index];

        int lastElementPos = --_count;
        ref HeapEntry lastElement = ref _heap[lastElementPos];

        if (lastElementPos > 0)
        {
            SiftDown(index, in lastElement.Element, in lastElement.Priority);
        }

        lastElement = default;
        _index.Remove(element);
    }

    private void UpdateIndex(int index, int newPriority)
    {
        T element;
        ref HeapEntry entry = ref _heap[index];

        if (newPriority == entry.Priority)
        {
            return;
        }

        element = entry.Element;
        if (newPriority < entry.Priority)
        {
            SiftUp(index, element, newPriority);
        }
        else
        {
            SiftDown(index, element, newPriority);
        }
    }

    private static void Resize(ref HeapEntry[] heap)
    {
        int newSize = heap.Length == 0 ? DefaultCapacity : 2 * heap.Length;
        Array.Resize(ref heap, newSize);
    }

    private void SiftUp(int index, in T element, in int priority)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) >> 2;
            ref HeapEntry parent = ref _heap[parentIndex];

            if (parent.Priority <= priority)
            {
                break;
            }

            _heap[index] = parent;
            _index[parent.Element] = index;
            index = parentIndex;
        }

        ref HeapEntry entry = ref _heap[index];
        entry.Element = element;
        entry.Priority = priority;
        _index[element] = index;
    }

    private void SiftDown(int index, in T element, in int priority)
    {
        int minChildIndex;
        var count = _count;
        var heap = _heap;

        while ((minChildIndex = (index << 2) + 1) < count)
        {
            // Find child with minimal priority
            ref HeapEntry minChild = ref heap[minChildIndex];
            var childUpperBound = Math.Min(count, minChildIndex + 4);

            for (var nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
            {
                ref HeapEntry nextChild = ref heap[nextChildIndex];
                if (nextChild.Priority < minChild.Priority)
                {
                    minChildIndex = nextChildIndex;
                    minChild = ref nextChild;
                }
            }

            // Compare with inserted priority
            if (priority <= minChild.Priority)
            {
                break;
            }

            heap[index] = minChild;
            _index[minChild.Element] = index;
            index = minChildIndex;
        }

        ref HeapEntry entry = ref heap[index];
        entry.Element = element;
        entry.Priority = priority;
        _index[element] = index;
    }

    private struct HeapEntry
    {
        public T Element;
        public int Priority;

        public void Deconstruct(out T element, out int priority)
        {
            element = Element;
            priority = Priority;
        }
    }
}