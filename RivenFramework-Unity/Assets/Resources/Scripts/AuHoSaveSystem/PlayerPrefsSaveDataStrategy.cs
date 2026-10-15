using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSaveDataStrategy : JsonBasedSaveDataStrategy
{
    public SerializableDictionary<string, string> currentSaveData = new();

    public override void Save(string fileName) => 
        PlayerPrefs.SetString(fileName, DataStringinator.ToDataString(currentSaveData));
    public override bool HasSave(string fileName) => PlayerPrefs.HasKey(fileName);
    public override void Load(string fileName)
    {
        string data = PlayerPrefs.GetString(fileName, null);
        if (string.IsNullOrEmpty(data))
            ClearValues();
        else
            currentSaveData = DataStringinator.FromDataString<SerializableDictionary<string, string>>(data);
    }
    public override void Clear(string fileName)
    {
        currentSaveData = new SerializableDictionary<string, string>();
        PlayerPrefs.DeleteKey(fileName);
    }

    protected override void SaveJsonValue(string value, string id)
    {
        currentSaveData.AddOrReplace(id, value);
    }
    protected override string LoadJsonValue(string id)
    {
        if (currentSaveData.TryGetValue(id, out string value))
            return value;
        return null;
    }

    public override void ClearValues() => currentSaveData = new SerializableDictionary<string, string>();
}
