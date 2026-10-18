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

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    Vector2 spawnLocal;
    float age;
    Vector2 knockOffset;
    Vector2 knockVelocity;
    float knockReturnAt;



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Vector2 LocalPosition => transform.localPosition;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public new void Awake()
    {
        base.Awake();
        physicsbody.isKinematic = true;
        OnPawnDeath += OnDeath;
        OnPawnHurt += OnHurt;
    }

    void Update()
    {
        age += Time.deltaTime;

        if (knockOffset != Vector2.zero && Time.time >= knockReturnAt)
        {
            knockOffset = Vector2.SmoothDamp(knockOffset, Vector2.zero, ref knockVelocity, knockReturnTime);
            if (knockOffset.sqrMagnitude < 0.0001f) knockOffset = Vector2.zero;
        }

        Vector2 pathOffset = movement ? movement.Offset(age) : Vector2.zero;
        transform.localPosition = spawnLocal + pathOffset + knockOffset;

        if (maxLifetime > 0f && age > maxLifetime) Despawn();
    }
    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    void OnDeath(DamageInfo info)
    {
        characterVisuals.transform.DOShakePosition(0.125f, new Vector3(0.05f, 0, 0));
        hurtParticleEmitter.Play();

        foreach (var renderer in spritesRenderers)
        {
            Flash(renderer);
        }
        Despawn();
    }

    void Despawn() => Destroy(gameObject, 0.15f);
    
    private void OnHurt(DamageInfo info = default)
    {
        characterVisuals.transform.DOShakePosition(0.125f, new Vector3(0.05f, 0, 0));
        hurtParticleEmitter.Play();

        foreach (var renderer in spritesRenderers)
        {
            Flash(renderer);
        }
    }
    
    
    public void Flash(SpriteRenderer targetRenderer)
    {
        targetRenderer.DOKill();

        Color original = originalColors[targetRenderer];
        targetRenderer.color = original;

        targetRenderer.DOColor(Color.yellow, flashDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetLink(targetRenderer.gameObject);
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
