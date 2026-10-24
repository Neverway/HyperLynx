//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using DG.Tweening;
using RivenFramework;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[Serializable]
public abstract class CutsceneEvent
{
    public string displayInfo;
    [Min(0)] public float duration = 0.5f;

    public abstract void Execute(CutsceneContext ctx);

    public virtual bool IsBlocking(CutsceneContext ctx) => false;

    public virtual void Cleanup(CutsceneContext ctx) { }
}

[Serializable]
public class ToggleObjectEvent : CutsceneEvent
{
    public GameObject[] targets;
    public bool active = true;

    public override void Execute(CutsceneContext ctx)
    {
        foreach (var target in targets)
        {
            if (!target) continue;
            var go = target;
            bool previous = go.activeSelf;
            ctx.RegisterRestore(() => { if (go) go.SetActive(previous); });
            go.SetActive(active);
        }
    }
}

[Serializable]
public class TogglePlayerEvent : CutsceneEvent
{
    public DSPawn_Player target;
    public bool active = true;

    public override void Execute(CutsceneContext ctx)
    {
        target = GameObject.FindObjectOfType<DSPawn_Player>();
        if (!target)
        {
            return;
        }
        var go = target.characterVisuals;
        bool previous = go.activeSelf;
        target.physicsbody.isKinematic = !active;
        ctx.RegisterRestore(() => { if (go) go.SetActive(previous); });
        go.SetActive(active);
    }
}

[Serializable]
public class PlayAnimationEvent : CutsceneEvent
{
    public Animator target;
    public bool targetIsLocalPlayer;
    public string stateName;
    public string triggerName;
    public int layer = 0;
    [Min(0)] public float crossFade = 0.1f;

    public override void Execute(CutsceneContext ctx)
    {
        if (!target && targetIsLocalPlayer) target = GameObject.FindObjectOfType<DSPawn_Player>().GetComponentInChildren<Animator>();
        if (!target) return;

        if (ctx.IsPreview && ctx.PreviewAnimators.Add(target))
            target.Rebind();

        if (!string.IsNullOrEmpty(triggerName))
            target.SetTrigger(triggerName);
        else if (crossFade > 0)
            target.CrossFadeInFixedTime(stateName, crossFade, layer);
        else
            target.Play(stateName, layer, 0f);

        if (ctx.IsPreview) target.Update(0f);
    }
}

[Serializable]
public class ShowTextboxEvent : CutsceneEvent
{
    public string title;
    [TextArea] public string content;
    public AudioClip audioClip;
    public bool waitForAudioClipToFinish;
    [Min(0)] public float textboxLifetime = 3f;
    public Transform targetTransform;
    public bool targetIsLocalPlayer;

    public override void Execute(CutsceneContext ctx)
    {
        if (ctx.IsPreview)
        {
            ctx.cutscene.previewCaption = $"{title}: {content}";
            return;
        }

        var handler = GameInstance.Get<GI_TextEventHandler>();
        handler.StartCoroutine(handler.CreateTextBoxEvent(
            title, content, audioClip, waitForAudioClipToFinish, textboxLifetime, targetTransform, targetIsLocalPlayer));
    }
}

[Serializable]
public class ShowChoiceboxEvent : CutsceneEvent
{
    public string query;
    public string choiceA, choiceB, choiceC;
    public UnityEvent onChoiceA = new UnityEvent();
    public UnityEvent onChoiceB = new UnityEvent();
    public UnityEvent onChoiceC = new UnityEvent();
    public bool waitForChoice = true;

    [NonSerialized] private bool answered;
    [NonSerialized] private SimpleChoicebox box;

    public override bool IsBlocking(CutsceneContext ctx) =>
        waitForChoice && !ctx.IsPreview && !answered;

    public override void Execute(CutsceneContext ctx)
    {
        answered = false;

        if (ctx.IsPreview)
        {
            ctx.cutscene.previewCaption = $"{query}  [{choiceA} / {choiceB} / {choiceC}]";
            return;
        }

        var a = new UnityEvent(); a.AddListener(() => { answered = true; onChoiceA?.Invoke(); });
        var b = new UnityEvent(); b.AddListener(() => { answered = true; onChoiceB?.Invoke(); });
        var c = new UnityEvent(); c.AddListener(() => { answered = true; onChoiceC?.Invoke(); });

        var handler = GameInstance.Get<GI_TextEventHandler>();
        box = handler.CreateChoiceBoxEvent(query, choiceA, choiceB, choiceC, a, b, c);
    }

    public override void Cleanup(CutsceneContext ctx)
    {
        if (box) Object.Destroy(box.gameObject);
    }
}

[Serializable]
public class MoveObjectEvent : CutsceneEvent
{
    public Transform target;
    public bool targetIsLocalPlayer;
    public Transform destination;
    public Vector3 position;
    [Min(0)] public float moveTime = 1f;
    public Ease ease = Ease.InOutSine;

    [NonSerialized] private Tween tween;

    public override void Execute(CutsceneContext ctx)
    {
        if (!target && targetIsLocalPlayer) target = GameObject.FindObjectOfType<DSPawn_Player>().transform;
        if (!target) return;

        var moved = target;
        Vector3 startPos = moved.position;
        ctx.RegisterRestore(() => { if (moved) moved.position = startPos; });

        Vector3 end = destination ? destination.position : position;
        tween = moved.DOMove(end, moveTime).SetEase(ease);
        ctx.Prepare(tween);
    }

    public override void Cleanup(CutsceneContext ctx)
    {
        if (tween != null && tween.IsActive()) tween.Kill();
    }
}