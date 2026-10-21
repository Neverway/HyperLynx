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
using UnityEngine;
using UnityEngine.Events;

public class ZoneWaveManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public List<ZoneWaveSpawner> waves = new List<ZoneWaveSpawner>();
    public UnityEvent onAllWavesCompleted;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private int currentWave;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Zone zone;
    public int CurrentWaveIndex => currentWave;
    public ZoneWaveSpawner CurrentWave => currentWave < waves.Count ? waves[currentWave] : null;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        zone = GetComponentInParent<Zone>();
        if (waves.Count == 0) waves.AddRange(GetComponentsInChildren<ZoneWaveSpawner>(true));

        foreach (var wave in waves) wave.OnWaveCompleted += HandleWaveCompleted;
    }

    private void Start()
    {
        if (!zone)
        {
            Debug.LogWarning("ZoneWaveManager couldn't find a Zone in its parents!", this);
            enabled = false;
            return;
        }

        zone.OnDepthChanged += HandleDepthChanged;

        TryStartCurrentWave();
    }

    private void OnDestroy()
    {
        if (zone) zone.OnDepthChanged -= HandleDepthChanged;
        foreach (var wave in waves)
        {
            if (wave) wave.OnWaveCompleted -= HandleWaveCompleted;
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void HandleDepthChanged(int _depth)
    {
        TryStartCurrentWave();
    }

    private void HandleWaveCompleted(ZoneWaveSpawner _wave)
    {
        if (CurrentWave != _wave) return;

        currentWave++;
        if (currentWave >= waves.Count)
        {
            onAllWavesCompleted.Invoke();
            return;
        }

        TryStartCurrentWave();
    }

    private void TryStartCurrentWave()
    {
        var wave = CurrentWave;
        if (wave == null || wave.State != ZoneWaveSpawner.WaveState.Pending) return;

        if (zone.currentDepth >= wave.triggerDepth) wave.StartWave();
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void StopAllWaves()
    {
        foreach (var wave in waves) wave.StopWave();
    }

    public void SetCurrentWave(int _index)
    {
        StopAllWaves();
        currentWave = Mathf.Clamp(_index, 0, waves.Count);
        TryStartCurrentWave();
    }


    #endregion
}
