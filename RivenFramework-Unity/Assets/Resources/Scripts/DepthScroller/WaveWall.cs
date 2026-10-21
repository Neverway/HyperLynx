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
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class WaveWall : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public ZoneWaveSpawner waveToClear;
    public bool autoDetectDepth = true;
    public int blockedDepth;
    public float openDuration = 0.4f;
    public UnityEvent onOpened;
    
    public float spinDegrees = 360f;
    public float shakeDuration = 0.5f;
    public float shakeStrength = 0.15f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private int resolvedDepth;
    private bool blocking;
    private bool isOpen;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Zone zone;
    public AudioClip destroy;
    public AudioSource audioSource;

    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        zone = GetComponentInParent<Zone>();
        if (!zone)
        {
            Debug.LogWarning("WaveWall needs to be inside a Zone", this);
            return;
        }

        resolvedDepth = ResolveDepth();

        
        if (waveToClear && waveToClear.State == ZoneWaveSpawner.WaveState.Completed)
        {
            Open(true);
            return;
        }

        zone.BlockDepth(resolvedDepth);
        blocking = true;
        if (waveToClear) waveToClear.OnWaveCompleted += HandleWaveCompleted;
    }

    private void OnDestroy()
    {
        if (waveToClear) waveToClear.OnWaveCompleted -= HandleWaveCompleted;
        if (blocking && zone) zone.UnblockDepth(resolvedDepth);
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private int ResolveDepth()
    {
        if (!autoDetectDepth) return blockedDepth;

        for (Transform t = transform; t != null; t = t.parent)
        {
            int index = zone.depthLayers.IndexOf(t.gameObject);
            if (index >= 0) return index;
        }

        Debug.LogWarning("WaveWall couldn't find a depth layer in its parents", this);
        return blockedDepth;
    }

    private void HandleWaveCompleted(ZoneWaveSpawner _wave)
    {
        Open(false);
    }

    private void Open(bool _instant)
    {
        if (isOpen) return;
        isOpen = true;

        if (blocking)
        {
            zone.UnblockDepth(resolvedDepth);
            blocking = false;
        }

        onOpened.Invoke();
        PlaySound(destroy);

        if (_instant || openDuration <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        var sequence = DOTween.Sequence().SetLink(gameObject);

        if (shakeDuration > 0f)
        {
            Vector3 basePosition = transform.localPosition;
            sequence.Append(DOTween.To(() => 0f, progress =>
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * shakeStrength;
                    transform.localPosition = basePosition + (Vector3)offset;
                }, 1f, shakeDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => transform.localPosition = basePosition));
        }

        sequence.Append(transform.DOScale(Vector3.zero, openDuration).SetEase(Ease.InBack));
        sequence.Join(transform.DOLocalRotate(new Vector3(0f, 0f, spinDegrees), openDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad));
        sequence.OnComplete(() => gameObject.SetActive(false));
    }
    
    private void PlaySound(AudioClip _clip)
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        if (_clip && audioSource) audioSource.PlayOneShot(_clip);
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void OpenWall() => Open(false);

    public void OpenWallInstant() => Open(true);
    
    public void SetOpen(bool _open)
    {
        if (_open) Open(false);
    }


    #endregion
}
