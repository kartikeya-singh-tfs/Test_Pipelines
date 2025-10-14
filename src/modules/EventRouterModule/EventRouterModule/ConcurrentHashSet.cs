using System.Collections;
using System.Collections.Concurrent;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Concurrent HashSet implementation using ConcurrentDictionary.
/// </summary>
/// <typeparam name="T">Type.</typeparam>
internal class ConcurrentHashSet<T> : IEnumerable<T>
{
    private readonly ConcurrentDictionary<T, byte> _dictionary;

    public ConcurrentHashSet()
    {
        _dictionary = new ConcurrentDictionary<T, byte>();
    }

    public ConcurrentHashSet(HashSet<T> set)
        : this()
    {
        foreach (var item in set)
        {
            _dictionary.TryAdd(item, byte.MinValue);
        }
    }

    public bool Add(T item)
    {
        return _dictionary.TryAdd(item, byte.MinValue);
    }

    public void UnionWith(HashSet<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public bool Remove(T item)
    {
        return _dictionary.TryRemove(item, out _);
    }

    public void ExceptWith(HashSet<T> items)
    {
        foreach (var item in items)
        {
            Remove(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var key in _dictionary.Keys)
        {
            yield return key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _dictionary.Count;
}
