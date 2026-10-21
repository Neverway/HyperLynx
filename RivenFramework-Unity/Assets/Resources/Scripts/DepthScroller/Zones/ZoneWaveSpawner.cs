//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class ZoneWaveSpawner : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public enum WaveState { Pending, InProgress, Completed }
    public enum CompletionMode { DefeatAll, SurviveDuration }
    
    [Header("Trigger")]
    public int triggerDepth;
    public float startDelay = 1f;
    
    [Header("Completion")]
    public CompletionMode completion = CompletionMode.DefeatAll;
    public float surviveDuration = 20f;
    public bool despawnEnemiesOnComplete = true;
    
    [Header("Spawns")]
    public List<SubWaveEntry> entries = new List<SubWaveEntry>();
    public bool loop;
    public float loopDelay = 5f;

    [Header("Events")]
    public UnityEvent onWaveStarted;
    public UnityEvent onWaveCompleted;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public WaveState State { get; private set; } = WaveState.Pending;
    public int EnemiesAlive => spawned.Count;
    public event Action<ZoneWaveSpawner> OnWaveCompleted;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private readonly List<SpawnEvent> timeline = new List<SpawnEvent>();
    private readonly List<DSPawn_Enemy> spawned = new List<DSPawn_Enemy>();
    private int nextEvent;
    private float timelineTime;
    private float elapsed;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Zone zone;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        zone = GetComponentInParent<Zone>();
        if (!zone)
        {
            Debug.LogWarning("ZoneWaveSpawner couldn't find a Zone in its parents! Put the waves under the zone object you dummy", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (State != WaveState.InProgress) return;

        elapsed += Time.deltaTime;
        timelineTime += Time.deltaTime;

        while (nextEvent < timeline.Count && timeline[nextEvent].time <= timelineTime)
        {
            SpawnEntry(timeline[nextEvent]);
            nextEvent++;
        }

        bool allSpawned = nextEvent >= timeline.Count;

        if (allSpawned && loop && completion == CompletionMode.SurviveDuration && timeline.Count > 0
            && timelineTime >= timeline[timeline.Count - 1].time + loopDelay)
        {
            nextEvent = 0;
            timelineTime = 0f;
            return;
        }

        spawned.RemoveAll(e => e == null);

        if (completion == CompletionMode.DefeatAll)
        {
            if (allSpawned && timelineTime >= 0f && spawned.Count == 0) CompleteWave();
        }
        else if (elapsed >= surviveDuration)
        {
            CompleteWave();
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var targetZone = zone ? zone : GetComponentInParent<Zone>();
        if (!targetZone) return;

        foreach (var entry in entries)
        {
            if (entry.depth < 0 || entry.depth >= targetZone.depthLayers.Count) continue;
            var layer = targetZone.depthLayers[entry.depth];
            if (!layer) continue;

            int count = Mathf.Max(1, entry.repeatCount);
            for (int i = 0; i < count; i++)
            {
                Vector2 p = entry.localPosition + entry.repeatOffset * i;
                Vector3 world = layer.transform.TransformPoint(p);
                Gizmos.color = Color.HSVToRGB((entry.depth * 0.15f) % 1f, 0.8f, 1f);
                Gizmos.DrawWireSphere(world, 0.35f);
                Handles.Label(world + Vector3.up * 0.5f, $"t={entry.time + entry.repeatInterval * i:0.0}s");
            }
        }
    }
    #endif



    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void BuildTimeline()
    {
        timeline.Clear();
        nextEvent = 0;

        foreach (var entry in entries)
        {
            if (entry.enemyPrefab == null) continue;

            int count = Mathf.Max(1, entry.repeatCount);
            for (int i = 0; i < count; i++)
            {
                timeline.Add(new SpawnEvent
                {
                    time = entry.time + entry.repeatInterval * i,
                    entry = entry,
                    index = i
                });
            }
        }

        timeline.Sort((a, b) => a.time.CompareTo(b.time));
    }

    private void SpawnEntry(SpawnEvent _event)
    {
        if (zone.depthLayers.Count == 0) return;

        var entry = _event.entry;
        int layer = Mathf.Clamp(entry.depth, 0, zone.depthLayers.Count - 1);
        Vector2 position = entry.localPosition + entry.repeatOffset * _event.index;

        var enemy = Instantiate(entry.enemyPrefab);
        enemy.Spawn(zone, layer, position);
        spawned.Add(enemy);
    }

    private void CompleteWave()
    {
        State = WaveState.Completed;

        if (despawnEnemiesOnComplete)
        {
            foreach (var enemy in spawned)
            {
                if (enemy) Destroy(enemy.gameObject);
            }
        }
        spawned.Clear();

        onWaveCompleted.Invoke();
        OnWaveCompleted?.Invoke(this);
    }



    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void StartWave()
    {
        if (State != WaveState.Pending) return;

        BuildTimeline();
        elapsed = 0f;
        timelineTime = -startDelay;
        State = WaveState.InProgress;
        onWaveStarted.Invoke();
    }

    public void StopWave()
    {
        if (State == WaveState.InProgress) State = WaveState.Pending;
    }


    #endregion
}

[Serializable]
public class SubWaveEntry
{
    public string displayInfo;
    
    [Header("Enemy")]
    [Tooltip("Seconds into the timeline to spawn the enemy")]
    public float time;
    [Tooltip("The enemy to spawn")]
    public DSPawn_Enemy enemyPrefab;
    [Tooltip("Which depth layer the enemy spawns on")]
    public int depth;
    [Tooltip("Where the enemy spawns in the depth layer's local space")]
    public Vector2 localPosition;
    
    [Header("Repeating Enemy Spawn")]
    [Tooltip("How many enemies to spawn")]
    public int repeatCount = 1;
    [Tooltip("Seconds between each copy spawn")]
    public float repeatInterval = 0.5f;
    [Tooltip("Local offset from last copy spawned")]
    public Vector2 repeatOffset;
}
 
public struct SpawnEvent
{
    public float time;
    public SubWaveEntry entry;
    public int index;
}
