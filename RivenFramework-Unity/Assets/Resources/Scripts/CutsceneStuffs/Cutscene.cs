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
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Cutscene : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeReference, Polymorphic]
    public List<CutsceneEvent> events = new List<CutsceneEvent>();
    public bool disableWhenFinished = true;
    public UnityEvent onFinished;
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [NonSerialized] public string previewCaption;
    public bool IsPlaying { get; private set; }
    public CutsceneContext Context { get; private set; }

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private int index;
    private float timer;
    private bool started;
    
    private struct SavedTransform
    {
        public Transform t;
        public Vector3 pos, scale;
        public Quaternion rot;
        public bool active;
    }
    private readonly List<SavedTransform> snapshot = new List<SavedTransform>();


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnEnable()
    {
        if (Application.isPlaying) Begin(new CutsceneContext());
    }

    private void Update()
    {
        if (Application.isPlaying) Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        if (Application.isPlaying && IsPlaying) Stop();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!string.IsNullOrEmpty(previewCaption))
            Handles.Label(transform.position + Vector3.up * 2f, previewCaption);
    }
#endif

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void Finish()
    {
        IsPlaying = false;
        if (Context.IsPreview) return;

        onFinished?.Invoke();
        if (disableWhenFinished) gameObject.SetActive(false);
    }

    private void TakeSnapshot()
    {
        snapshot.Clear();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            snapshot.Add(new SavedTransform
            {
                t = t,
                pos = t.localPosition,
                rot = t.localRotation,
                scale = t.localScale,
                active = t.gameObject.activeSelf
            });
        }
    }

    private void RestoreSnapshot()
    {
        foreach (var s in snapshot)
        {
            if (!s.t) continue;
            s.t.localPosition = s.pos;
            s.t.localRotation = s.rot;
            s.t.localScale = s.scale;
            s.t.gameObject.SetActive(s.active);
        }
        snapshot.Clear();
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Begin(CutsceneContext _context)
    {
        Context = _context;
        Context.cutscene = this;

        if (Context.IsPreview)
        {
            TakeSnapshot();
            gameObject.SetActive(true);
        }

        index = 0;
        timer = 0;
        started = false;
        IsPlaying = true;
        Tick(0f);
    }

    public void Tick(float _deltaTime)
    {
        if (!IsPlaying) return;

        while (index < events.Count)
        {
            var e = events[index];
            if (e != null)
            {
                if (!started)
                {
                    started = true;
                    timer = 0;
                    try { e.Execute(Context); }
                    catch (Exception ex) { Debug.LogException(ex, this); }
                }

                timer += _deltaTime;
                _deltaTime = 0;

                if (timer < e.duration || e.IsBlocking(Context)) return;
                _deltaTime = timer - e.duration;
            }

            started = false;
            index++;
        }

        Finish();
    }

    public void Stop()
    {
        IsPlaying = false;
        if (Context == null) return;

        foreach (var e in events)
        {
            try { e?.Cleanup(Context); } catch (Exception ex) { Debug.LogException(ex, this); }
        }

        if (Context.IsPreview)
        {
            Context.RunRestore();
            RestoreSnapshot();
            previewCaption = null;
        }
    }

    #endregion
}

public class CutsceneContext
{
    public Cutscene cutscene;
    public bool IsPreview;
    public Action<Tween> PrepareTween;
    public readonly HashSet<Animator> PreviewAnimators = new HashSet<Animator>();
    private readonly List<Action> restoreActions = new List<Action>();
    public void RegisterRestore(Action _restore)
    {
        if (IsPreview) restoreActions.Add(_restore);
    }
    public void RunRestore()
    {
        for (int i = restoreActions.Count - 1; i >= 0; i--)
        {
            try { restoreActions[i](); } catch (Exception e) { Debug.LogException(e); }
        }
        restoreActions.Clear();
        PreviewAnimators.Clear();
    }
    public void Prepare(Tween _tween) => PrepareTween?.Invoke(_tween);
}