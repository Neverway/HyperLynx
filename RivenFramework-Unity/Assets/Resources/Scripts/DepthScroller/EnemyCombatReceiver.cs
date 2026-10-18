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

[RequireComponent(typeof(DSPawn_Enemy))]
public class EnemyCombatReceiver : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Hitbox")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0f, 0f);

    [Header("Squish")]
    public Transform visuals;
    public float squishScaleX = 1.35f;
    public float squishScaleY = 0.35f;
    public float squishDownTime = 0.07f;
    public float squishHoldTime = 0.05f;
    public float squishRecoverTime = 0.4f;

    [Header("Stomp")]
    public float stompCooldown = 0.3f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public static readonly List<EnemyCombatReceiver> Active = new List<EnemyCombatReceiver>();
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Vector3 baseScale = Vector3.one;
    private Sequence squishSequence;
    private Tween knockTween;
    private float stompableAt;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public DSPawn_Enemy Enemy { get; private set; }
    public int Depth => Enemy.depth;
    public Vector2 Center => Enemy.LocalPosition + hitboxOffset;
    public Rect Bounds => new Rect(Center - hitboxSize * 0.5f, hitboxSize);
    public float Top => Center.y + hitboxSize.y * 0.5f;
    public bool CanBeStomped => Time.time >= stompableAt;

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        Enemy = GetComponent<DSPawn_Enemy>();
        if (!visuals) visuals = transform;
        baseScale = visuals.localScale;
    }

    private void OnEnable()
    {
        if (!Active.Contains(this)) Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    private void OnDestroy()
    {
        squishSequence?.Kill();
        knockTween?.Kill();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.TransformPoint(hitboxOffset), hitboxSize);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void TakeKick(float _damage, Vector2 _knockback)
    {
        Enemy.ModifyHealth(-_damage);
        Knock(_knockback, 0.2f);
    }

    public void TakeStomp(float _damage)
    {
        stompableAt = Time.time + stompCooldown;
        Enemy.ModifyHealth(-_damage);
        Squish();
    }

    public void Knock(Vector2 _delta, float _duration)
    {
        knockTween?.Kill();

        float applied = 0f;
        knockTween = 
            DOTween.To(() => applied, v =>
                {
                    float step = v - applied; applied = v; Enemy.Nudge(_delta * step);
                }, 1f, _duration)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject);
    }

    public void Squish()
    {
        squishSequence?.Kill();
        visuals.localScale = baseScale;

        var squished = new Vector3(baseScale.x * squishScaleX, baseScale.y * squishScaleY, baseScale.z);

        squishSequence = DOTween.Sequence().SetLink(gameObject);
        squishSequence.Append(visuals.DOScale(squished, squishDownTime).SetEase(Ease.OutQuad));
        squishSequence.AppendInterval(squishHoldTime);
        squishSequence.Append(visuals.DOScale(baseScale, squishRecoverTime).SetEase(Ease.OutElastic));
    }

    #endregion
}
