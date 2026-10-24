//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GlitchyTransitions : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Appear")]
    public float appearDuration = 0.35f;
    public Vector3 collapsedScale = new Vector3(1.3f, 0.02f, 1f);
    public float appearShakeStrength = 0.1f;

    [Header("Disappear")]
    public float disappearDuration = 0.25f;
    public float disappearShakeStrength = 0.15f;

    [Header("Style")]
    public bool steppedScale = true;
    [Min(1)] public int scaleSteps = 6;
    public bool useUnscaledTime;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Transform root;
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Sequence activeSequence;
    private bool quitting;
    private readonly List<SpriteRenderer> renderers = new();


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public GameObject visualRoot;
    public Material normalSpriteMaterial;
    public Material glitchSpriteMaterial;
    private bool started;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        root = visualRoot != null ? visualRoot.transform : transform;
        originalLocalPosition = root.localPosition;
        originalLocalScale = root.localScale;
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    private void Start()
    {
        started = true;
        PlayAppear();
    }

    public void OnEnable()
    {
        if (!started) return;
        PlayAppear();
    }

    public void OnDisable()
    {
        KillActiveSequence();

        if (started && !quitting && gameObject.scene.isLoaded) SpawnDisappearGhost();

        ResetVisual();
        SetMaterial(normalSpriteMaterial);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void KillActiveSequence()
    {
        if (activeSequence != null && activeSequence.IsActive()) activeSequence.Kill();
        activeSequence = null;
    }
    
    private void PlayAppear()
    {
        KillActiveSequence();
        ResetVisual();

        SetMaterial(glitchSpriteMaterial);

        root.localScale = Vector3.Scale(originalLocalScale, collapsedScale);
        activeSequence = PlayScaleAndShake(root, originalLocalScale, originalLocalPosition, appearDuration, appearShakeStrength);

        activeSequence.OnComplete(() =>
        {
            activeSequence = null;
            SetMaterial(normalSpriteMaterial);
        });
    }

    private void ResetVisual()
    {
        root.localPosition = originalLocalPosition;
        root.localScale = originalLocalScale;
    }

    private void SetMaterial(Material material)
    {
        if (material == null) return;

        root.GetComponentsInChildren(true, renderers);
        foreach (var r in renderers)
        {
            r.sharedMaterial = material;

            if (material == glitchSpriteMaterial && !r.TryGetComponent<SpriteGlitchRect>(out _))
                r.gameObject.AddComponent<SpriteGlitchRect>();
        }
    }

    private Sequence PlayScaleAndShake(Transform target, Vector3 toScale, Vector3 originLocalPos, float duration, float shakeStrength)
    {
        var seq = DOTween.Sequence();

        var scaleTween = target.DOScale(toScale, duration);
        if (steppedScale) scaleTween.SetEase(SteppedEase);
        else scaleTween.SetEase(Ease.OutQuad);
        seq.Append(scaleTween);

        if (shakeStrength > 0f)
        {
            seq.Join(DOVirtual.Float(1f, 0f, duration, fade =>
            {
                target.localPosition = originLocalPos + (Vector3)(Random.insideUnitCircle * (shakeStrength * fade));
            }).SetEase(Ease.Linear));
        }

        seq.SetUpdate(useUnscaledTime).SetLink(target.gameObject);
        return seq;
    }

    private float SteppedEase(float time, float duration, float overshootOrAmplitude, float period)
    {
        return Mathf.Floor(time / duration * scaleSteps) / scaleSteps;
    }

    private void SpawnDisappearGhost()
    {
        var ghostRoot = new GameObject(name + "_GlitchGhost").transform;
        ghostRoot.SetPositionAndRotation(root.position, root.rotation);
        ghostRoot.localScale = root.lossyScale;

        root.GetComponentsInChildren(true, renderers);
        foreach (var r in renderers)
        {
            if (!r.enabled || r.sprite == null || !r.gameObject.activeSelf) continue;

            var g = new GameObject(r.name).transform;
            g.SetPositionAndRotation(r.transform.position, r.transform.rotation);
            g.localScale = r.transform.lossyScale;
            g.SetParent(ghostRoot, true);

            var gr = g.gameObject.AddComponent<SpriteRenderer>();
            gr.sprite = r.sprite;
            gr.color = r.color;
            gr.flipX = r.flipX;
            gr.flipY = r.flipY;
            gr.sortingLayerID = r.sortingLayerID;
            gr.sortingOrder = r.sortingOrder;
            gr.drawMode = r.drawMode;
            if (r.drawMode != SpriteDrawMode.Simple) gr.size = r.size;
            gr.sharedMaterial = glitchSpriteMaterial;

            g.gameObject.AddComponent<SpriteGlitchRect>();
        }

        var fullScale = ghostRoot.localScale;
        ghostRoot.localScale = fullScale;
        PlayScaleAndShake(ghostRoot, Vector3.Scale(fullScale, collapsedScale), ghostRoot.localPosition,
                disappearDuration, disappearShakeStrength)
            .OnComplete(() => Destroy(ghostRoot.gameObject));
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
