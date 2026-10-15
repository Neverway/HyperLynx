using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public List<TKey> Keys => keys;
    public List<TValue> Values => values;
    public int Count => keys.Count;

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException($"Key '{key}' already exists in dictionary.");

        keys.Add(key);
        values.Add(value);
    }
    public void AddOrReplace(TKey key, TValue value)
    {
        if (ContainsKey(key))
            this[key] = value;
        else
            Add(key, value);
    }
    public bool Remove(TKey key)
    {
        if (!ContainsKey(key))
            return false;

        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
            return true;
        }
        return false;
    }
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!ContainsKey(key))
        {
            value = default;
            return false;
        }

        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            value = values[index];
            return true;
        }
        value = default;
        return false;
    }
    public bool ContainsKey(TKey key) => keys.Contains(key);
    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public TValue this[TKey key]
    {
        get => values[keys.IndexOf(key)];
        set => values[keys.IndexOf(key)] = value;
    }
    public TValue[] this[params TKey[] keys]
    {
        get
        {
            TValue[] values = new TValue[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                values[i] = this[keys[i]];
            return values;
        }
        set
        {
            for (int i = 0; i < keys.Length; i++)
                this[keys[i]] = value[i];
        }
    }



    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    //Dictionary conversion ---------------------------------------------------------------------------------------------
    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            dictionary[keys[i]] = values[i];
        return dictionary;
    }
    public void FromDictionary(Dictionary<TKey, TValue> dict)
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
}