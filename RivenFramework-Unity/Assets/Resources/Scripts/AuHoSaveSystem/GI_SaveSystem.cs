using ErryLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RivenFramework;
using Unity.VisualScripting;
using UnityEngine;

public class GI_SaveSystem : MonoBehaviour
{
    [Polymorphic, SerializeReference] public SaveDataStrategy saveDataStrategy;

    public bool doSaving = true;
    public string gameSaveName = "AuHo";
    public int saveSlot = 0;
    public string defaultWorld = "Town";

    public string SaveDataFileName => $"{gameSaveName}_{saveSlot}_Savedata";

    private List<Tuple<SaveAndLoadPropertyAttribute, PropertyInfo>> cachedSaveLoadProperties;
    private List<MethodInfo> cachedInvokeBeforeSaveMethods;
    private List<MethodInfo> cachedInvokeAfterLoadMethods;
    private List<MethodInfo> cachedInvokeOnNewGameMethods;
    private MethodInfo saveValueMethod;
    private MethodInfo loadValueMethod;
    [Reload] private static GI_SaveSystem instance;

    public static SavingType CurrentSavingType { get; private set; }
    public enum SavingType { None, SwitchingScenes, SavingOrLoadingFile }

    public void Start()
    {
        if (instance == null)
            instance = this;

        CacheSaveLoadProperties();
        CacheInvokeOnSaveAndLoadMethods();
        CacheSaveLoadValueMethods();
    }
    public void Update()
    {
        Tuple<KeyCode, int>[] saveSlotKeys = {
            new(KeyCode.Alpha1, 1),
            new(KeyCode.Alpha2, 2),
            new(KeyCode.Alpha3, 3),
            new(KeyCode.Alpha4, 4),
            new(KeyCode.Alpha5, 5),
            new(KeyCode.Alpha6, 6),
            new(KeyCode.Alpha7, 7),
            new(KeyCode.Alpha8, 8),
            new(KeyCode.Alpha9, 9),
            new(KeyCode.Alpha0, 0),
        };
        foreach(var saveSlotKey in saveSlotKeys)
        {
            if (Input.GetKeyDown(saveSlotKey.Item1))
            {
                saveSlot = saveSlotKey.Item2;
                
                // Show textbox for switching file
                var textEvent = new TextEvent();
                //textEvent.AddFrame($"Switched to [ File {saveSlot} ]");
                //textEvent.TryDisplay();
                Debug.Log($"Switched to [ File {saveSlot} ]");
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
                OnSaveGame();
            if (Input.GetKeyDown(KeyCode.L))
                OnLoadGame();
            if (Input.GetKeyDown(KeyCode.Backspace))
                OnClearSave();
        }
    }

    private void CacheSaveLoadProperties()
    {
        cachedSaveLoadProperties = new List<Tuple<SaveAndLoadPropertyAttribute, PropertyInfo>>();

        foreach (var attribute in ReflectionCache.GetAttributeUsageInfos<SaveAndLoadPropertyAttribute>())
        {
            if (attribute.Member is PropertyInfo property)
            {
                if (!property.IsStatic())
                {
                    Debug.LogError($"{property.DeclaringType}.{property.Name}: You cant put a " +
                        $"SaveAndLoadProperty attribute on a non-static property");
                    continue;
                }
                if (!property.CanRead || !property.CanWrite)
                {
                    Debug.LogError($"{property.DeclaringType}.{property.Name}: The property with the " +
                        $"SaveAndLoadProperty attribute must have a Set AND a Get method");
                    continue;
                }
                var castedAttribute = attribute.As<SaveAndLoadPropertyAttribute>();
                var toCache = new Tuple<SaveAndLoadPropertyAttribute, PropertyInfo>(castedAttribute, property);
                cachedSaveLoadProperties.Add(toCache);
            }
            else
                Debug.LogError("You cant put a SaveAndLoadProperty attribute on a non-property");
        }
    }
    private void CacheInvokeOnSaveAndLoadMethods()
    {
        //[InvokeBeforeSave] method caching
        {
            cachedInvokeBeforeSaveMethods = new List<MethodInfo>();
            AttributeInfo[] saveAttributes = ReflectionCache.GetAttributeUsageInfos<InvokeBeforeSaveAttribute>()
                .OrderBy(at => at.As<InvokeBeforeSaveAttribute>().priority).ToArray();
            foreach (var attribute in saveAttributes)
            {
                if (attribute.Member is MethodInfo method)
                {
                    if (!method.IsStatic())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: You cant put an " +
                            $"InvokeBeforeSave attribute on a non-static method");
                        continue;
                    }
                    if (method.ContainsGenericParameters || !method.HasParametersNone())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: This method with the " +
                            $"InvokeBeforeSave attribute must have no parameters of any kind");
                        continue;
                    }
                    cachedInvokeBeforeSaveMethods.Add(method);
                }
                else
                    Debug.LogError("You cant put an InvokeBeforeSave attribute on a non-property");
            }
        }

        //[InvokeAfterLoad] method caching
        {
            cachedInvokeAfterLoadMethods = new List<MethodInfo>();
            AttributeInfo[] loadAttributes = ReflectionCache.GetAttributeUsageInfos<InvokeAfterLoadAttribute>()
                .OrderBy(at => at.As<InvokeAfterLoadAttribute>().priority).ToArray();

            foreach (var attribute in loadAttributes)
            {
                if (attribute.Member is MethodInfo method)
                {
                    if (!method.IsStatic())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: You cant put an " +
                            $"InvokeAfterLoad attribute on a non-static method");
                        continue;
                    }
                    if (method.ContainsGenericParameters || !method.HasParametersNone())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: This method with the " +
                            $"InvokeAfterLoad attribute must have no parameters of any kind");
                        continue;
                    }
                    cachedInvokeAfterLoadMethods.Add(method);
                }
                else
                    Debug.LogError("You cant put an InvokeAfterLoad attribute on a non-property");
            }
        }

        //[InvokeOnNewGame] method caching
        {
            cachedInvokeOnNewGameMethods = new List<MethodInfo>();
            AttributeInfo[] newGameAttributes = ReflectionCache.GetAttributeUsageInfos<InvokeBeforeNewGameAttribute>()
                .OrderBy(at => at.As<InvokeBeforeNewGameAttribute>().priority).ToArray();

            foreach (var attribute in newGameAttributes)
            {
                if (attribute.Member is MethodInfo method)
                {
                    if (!method.IsStatic())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: You cant put an " +
                            $"InvokeOnNewGame attribute on a non-static method");
                        continue;
                    }
                    if (method.ContainsGenericParameters || !method.HasParametersNone())
                    {
                        Debug.LogError($"{method.DeclaringType}.{method.Name}: This method with the " +
                            $"InvokeOnNewGame attribute must have no parameters of any kind");
                        continue;
                    }
                    cachedInvokeOnNewGameMethods.Add(method);
                }
                else
                    Debug.LogError("You cant put an InvokeOnNewGame attribute on a non-property");
            }
        }
    }
    private void CacheSaveLoadValueMethods()
    {
        saveValueMethod = typeof(GI_SaveSystem)
            .GetMethod(nameof(SaveValue), BindingFlags.Static | BindingFlags.Public);
        loadValueMethod = typeof(GI_SaveSystem)
            .GetMethod(nameof(LoadValue), BindingFlags.Static | BindingFlags.Public);
    }
    
    [ContextMenu("Trigger Save Game")]
    private void OnSaveGame()
    {
        if (!Application.isPlaying || !doSaving) return;

        Debug.Log("<size=24>SAVED GAME</size>");

        CurrentSavingType = SavingType.SavingOrLoadingFile;
        {
            //Grab values from attributes made for this save system
            SaveValuesFromAttributes();

            //Commit all values to PlayerPrefs
            saveDataStrategy.Save(SaveDataFileName);
        }
        CurrentSavingType = SavingType.None;
        
        // Show textbox for saving file
        var textEvent = new TextEvent();
        
        var gameState = GameInstance.Get<GI_HyLiGameState>().saveData;
        var playtime = gameState.playtime;
        var formatedTime = TimeSpan.FromSeconds(playtime);
        //textEvent.AddFrame($"[ File {saveSlot} ] \n {formatedTime:hh':'mm':'ss} \n Game has been saved!");
        //textEvent.TryDisplay();
        Debug.Log($"[ File {saveSlot} ] \n {formatedTime:hh':'mm':'ss} \n Game has been saved!");
    }
    
    [ContextMenu("Trigger Load Game")]
    private void OnLoadGame()
    {
        Debug.Log("Load");
        if (!Application.isPlaying || !doSaving) return;
        Debug.Log("Load 1");

        if (!HasSave())
        {
            CurrentSavingType = SavingType.SavingOrLoadingFile;
            Debug.Log($"NO SAVE FOUND, CLEARING VALUES, LOADING {defaultWorld}");
            saveDataStrategy.ClearValues();
            ResetValuesFromAttributes();
            GameInstance.Get<GI_WorldLoader>().LoadWorld(defaultWorld);
            CurrentSavingType = SavingType.None;
            return;
        }

        CurrentSavingType = SavingType.SavingOrLoadingFile;
        {
            Debug.Log("Load 2");
            //Load all values from PlayerPrefs
            saveDataStrategy.Load(SaveDataFileName);

            //Apply values to attributes made for this save system
            LoadValuesFromAttributes();
        }
        CurrentSavingType = SavingType.None;

    }
    
    [ContextMenu("Trigger Clear Save")]
    public void OnClearSave()
    {
        if (!Application.isPlaying) return;

        saveDataStrategy.Clear(SaveDataFileName);
        
        // Show textbox for erasing file
        var textEvent = new TextEvent();
        //textEvent.AddFrame($"[ File {saveSlot} ] has been erased!");
        //textEvent.TryDisplay();
        Debug.Log($"[ File {saveSlot} ] has been erased!");
    }

    public bool HasSave() => saveDataStrategy.HasSave(SaveDataFileName);

    [ContextMenu("Remove All PlayerPrefs")]
    private void RemoveAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    private void SaveValuesFromAttributes()
    {
        //Call all methods with InvokeBeforeSave attributes
        foreach (var beforeSaveMethod in cachedInvokeBeforeSaveMethods)
            beforeSaveMethod.Invoke(null, null);

        PlayerPrefs.Save();
        //Save all values from properties with SaveAndLoadProperty attributes
        foreach (var saveLoadProperty in cachedSaveLoadProperties)
        {
            var value = saveLoadProperty.Item2.GetValue(null);
            var saveMethod = saveValueMethod.MakeGenericMethod(saveLoadProperty.Item2.PropertyType);
            saveMethod.Invoke(this, new object[] { value, saveLoadProperty.Item1.saveId });
        }
    }
    private void LoadValuesFromAttributes()
    {
        //Load all values to properties with SaveAndLoadProperty attributes
        foreach (var saveLoadProperty in cachedSaveLoadProperties)
        {
            var value = saveLoadProperty.Item2.GetValue(null);
            var loadMethod = loadValueMethod.MakeGenericMethod(saveLoadProperty.Item2.PropertyType);
            var loadedValue = loadMethod.Invoke(this, new object[] { value, saveLoadProperty.Item1.saveId });
            saveLoadProperty.Item2.SetValue(null, loadedValue);
        }

        //Call all methods with InvokeAfterLoad attributes
        foreach (var afterLoadMethod in cachedInvokeAfterLoadMethods)
            afterLoadMethod.Invoke(null, null);
    }
    private void ResetValuesFromAttributes()
    {
        //Load all values to properties with SaveAndLoadProperty attributes
        foreach (var saveLoadProperty in cachedSaveLoadProperties)
        {
            saveLoadProperty.Item2.SetValue(null, null);
        }
        //Call all methods with InvokeAfterLoad attributes
        foreach (var onNewGameMethod in cachedInvokeOnNewGameMethods)
            onNewGameMethod.Invoke(null, null);
    }


    public static void SaveGame() => instance.OnSaveGame();
    public static void LoadGame() => instance.OnLoadGame();
    public static void NotifyLeavingScene()
    {
        if (CurrentSavingType == SavingType.SavingOrLoadingFile)
            return;

        CurrentSavingType = SavingType.SwitchingScenes;
        instance.SaveValuesFromAttributes();
        CurrentSavingType = SavingType.None;
    }
    public static void NotifyEnteredScene()
    {
        CurrentSavingType = SavingType.SwitchingScenes;
        instance.LoadValuesFromAttributes();
        CurrentSavingType = SavingType.None;
    }

    public static void SaveValue<T>(T value, string id) => instance.saveDataStrategy.WriteValue(value, id);
    public static T LoadValue<T>(T defaultValue, string id) => instance.saveDataStrategy.ReadValue(defaultValue, id);
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SaveAndLoadPropertyAttribute : Attribute 
{
    public string saveId;
    public SaveAndLoadPropertyAttribute(string saveId)
    {
        this.saveId = saveId;
    }
}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InvokeBeforeSaveAttribute : Attribute 
{
    public int priority = 0;
    public InvokeBeforeSaveAttribute(int priority = 0) => this.priority = priority;
}


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InvokeAfterLoadAttribute : Attribute 
{
    public int priority = 0;
    public InvokeAfterLoadAttribute(int priority = 0) => this.priority = priority;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InvokeBeforeNewGameAttribute : Attribute
{
    public int priority = 0;
    public InvokeBeforeNewGameAttribute(int priority = 0) => this.priority = priority;
}