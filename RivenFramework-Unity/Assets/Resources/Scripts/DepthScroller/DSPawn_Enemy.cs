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

public class DSPawn_Enemy : DSPawn
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public int depth;
    public EnemyMovement movement;
    public EnemyAttack attack;
    public float maxLifetime = 0f;
    [Header("Knockback Recovery")]
    public float knockReturnDelay = 0.15f;
    public float knockReturnTime = 0.5f;
    public float maxKnockDistance = 4f;
    [Header("Hitstun")]
    public float hitstunDuration = 0.2f;
    public float shakeStrength = 0.1f;
    public Color hitFlashColor = Color.yellow;
    public AudioClip hurt;
    public AudioSource audioSource;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    Vector2 spawnLocal;
    float age;
    Vector2 knockOffset;
    Vector2 knockVelocity;
    float knockReturnAt;
    float stunnedUntil;
    int lastFeedbackFrame = -1;
    Transform shakeTarget;
    Vector3 shakeTargetBase;
    bool shaking;

    public bool IsStunned => Time.time < stunnedUntil;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Vector2 LocalPosition => transform.localPosition;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public new void Awake()
    {
        base.Awake();
        shakeTarget = characterVisuals ? characterVisuals.transform : transform;
        if (shakeTarget != transform) shakeTargetBase = shakeTarget.localPosition;
        physicsbody.isKinematic = true;
        OnPawnDeath += OnDeath;
        OnPawnHurt += OnHurt;
        
        var zone = GetComponentInParent<Zone>();
        if (zone)
        {
            for (Transform t = transform; t != null; t = t.parent)
            {
                int index = zone.depthLayers.IndexOf(t.gameObject);
                if (index >= 0) { depth = index; break; }
            }
            spawnLocal = transform.localPosition;
            if (attack) StartCoroutine(attack.Run(this));
        }
    }

    void Update()
    {
        if (!IsStunned) age += Time.deltaTime;

        if (knockOffset != Vector2.zero && Time.time >= knockReturnAt)
        {
            knockOffset = Vector2.SmoothDamp(knockOffset, Vector2.zero, ref knockVelocity, knockReturnTime);
            if (knockOffset.sqrMagnitude < 0.0001f) knockOffset = Vector2.zero;
        }

        Vector2 pathOffset = movement ? movement.Offset(age) : Vector2.zero;
        Vector3 position = spawnLocal + pathOffset + knockOffset;

        Vector3 shake = Vector3.zero;
        if (IsStunned && hitstunDuration > 0f)
        {
            float strength = (stunnedUntil - Time.time) / hitstunDuration;
            shake = Random.insideUnitCircle * (shakeStrength * strength);
        }

        if (shakeTarget == transform)
        {
            transform.localPosition = position + shake;
        }
        else
        {
            transform.localPosition = position;

            if (shake != Vector3.zero || shaking)
            {
                shakeTarget.localPosition = shakeTargetBase + shake;
                shaking = shake != Vector3.zero;
            }
        }

        if (maxLifetime > 0f && age > maxLifetime) Despawn();
    }
    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    void Despawn() => Destroy(gameObject, 0.15f);
    
    void OnDeath(DamageInfo info)
    {
        PlayHitFeedback();
        Despawn();
    }

    private void OnHurt(DamageInfo info = default)
    {
        PlayHitFeedback();
    }

    public void PlayHitFeedback()
    {
        PlaySound(hurt);
        if (lastFeedbackFrame == Time.frameCount) return;
        lastFeedbackFrame = Time.frameCount;

        stunnedUntil = Time.time + hitstunDuration;
        if (hurtParticleEmitter) hurtParticleEmitter.Play();

        foreach (var renderer in spritesRenderers)
        {
            Flash(renderer);
        }
    }

    public void Flash(SpriteRenderer targetRenderer)
    {
        targetRenderer.DOKill(true);

        targetRenderer.DOColor(hitFlashColor, flashDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetLink(targetRenderer.gameObject);
    }
    
    private void PlaySound(AudioClip _clip)
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        if (_clip && audioSource) audioSource.PlayOneShot(_clip);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Spawn(Zone zone, int layer, Vector2 localPos)
    {
        depth = layer;
        spawnLocal = localPos;
        transform.SetParent(zone.depthLayers[layer].transform, false);
        transform.localPosition = localPos;
        zone.TintNewObject(transform, layer);
        age = 0f;
        if (attack) StartCoroutine(attack.Run(this));
    }
    
    public void Nudge(Vector2 _delta)
    {
        knockOffset = Vector2.ClampMagnitude(knockOffset + _delta, maxKnockDistance);
        knockVelocity = Vector2.zero;
        knockReturnAt = Time.time + knockReturnDelay;
    }


    #endregion
}
