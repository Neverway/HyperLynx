using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class JsonBasedSaveDataStrategy : SaveDataStrategy
{
    public override void WriteValue<T>(T value, string id)
    {
        string stringValue = DataStringinator.ToDataString(value);
        SaveJsonValue(stringValue, id);
    }
    public override T ReadValue<T>(T defaultValue, string id)
    {
        string stringValue = LoadJsonValue(id);
        if (stringValue == null) return defaultValue;
        return DataStringinator.FromDataString<T>(stringValue);
    }

    protected abstract void SaveJsonValue(string value, string id);
    protected abstract string LoadJsonValue(string id);
}
[Serializable]
public class Wrapper<T>
{
    public Wrapper(T value) { this.value = value; }
    public T value;
}