using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataStringinator
{
    public static string ToDataString<T>(T obj)
    {
        return JsonUtility.ToJson(obj);
    }
    public static T FromDataString<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}

public interface IDataStringable
{
    public string ToDataString();
    public void FromDataString(string json);
}