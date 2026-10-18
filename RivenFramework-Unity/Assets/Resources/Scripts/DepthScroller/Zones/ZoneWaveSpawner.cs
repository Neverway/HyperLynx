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

public class ZoneWaveSpawner : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public List<WaveEntry> entries = new List<WaveEntry>();
    public bool loop;
    public float loopDelay = 5f;
    public float startDelay = 1f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<SpawnEvent> timeline = new List<SpawnEvent>();
    private int nextEvent;
    private float timer;
    private bool finished;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Zone zone;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        if (!zone) zone = GetComponent<Zone>();
        if (!zone) zone = GetComponentInParent<Zone>();
        if (!zone)
        {
            Debug.LogWarning("Wavespawner couldn't locate its zone! Make sure you put the ZoneWaveSpawner on the zone object", this);
            enabled = false;
            return;
        }
 
        BuildTimeline();
        timer = -startDelay;

    }

    
    private void Update()
    {
        if (finished || timeline.Count == 0) return;
 
        timer += Time.deltaTime;
 
        while (nextEvent < timeline.Count && timeline[nextEvent].time <= timer)
        {
            SpawnEntry(timeline[nextEvent]);
            nextEvent++;
        }
 
        if (nextEvent >= timeline.Count)
        {
            if (loop)
            {
                float lastTime = timeline[timeline.Count - 1].time;
                if (timer >= lastTime + loopDelay)
                {
                    nextEvent = 0;
                    timer = 0f;
                }
            }
            else
            {
                finished = true;
            }
        }
    }
    
    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var targetZone = zone ? zone : GetComponentInParent<Zone>();
            if (!targetZone) targetZone = GetComponentInChildren<Zone>();
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
        finished = false;
 
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
        var entry = _event.entry;
        int layer = Mathf.Clamp(entry.depth, 0, zone.depthLayers.Count - 1);
        if (zone.depthLayers.Count == 0) return;
 
        Vector2 position = entry.localPosition + entry.repeatOffset * _event.index;
 
        var enemy = Instantiate(entry.enemyPrefab);
        enemy.Spawn(zone, layer, position);
    }



    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Restart()
    {
        BuildTimeline();
        timer = -startDelay;
    }


    #endregion
}

[Serializable]
public class WaveEntry
{
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
    public WaveEntry entry;
    public int index;
}
