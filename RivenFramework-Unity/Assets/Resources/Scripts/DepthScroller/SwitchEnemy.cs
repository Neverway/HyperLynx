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
using UnityEngine.Events;

[RequireComponent(typeof(EnemyCombatReceiver))]
public class SwitchEnemy : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool isOn;
    public bool latchOn = true;
    public float hitCooldown = 0.25f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public LogicOutput<bool> output;
    public UnityEvent onTurnedOn;
    public UnityEvent onTurnedOff;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private float nextHitAt;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    private EnemyCombatReceiver receiver;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        receiver = GetComponent<EnemyCombatReceiver>();
        receiver.ignoreDamage = true;
        receiver.allowKnockback = false;
    }

    private void OnEnable()
    {
        receiver.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        receiver.OnHit -= HandleHit;
    }

    private void Start()
    {
        ApplyVisuals();
        output.Set(isOn);
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void HandleHit()
    {
        if (Time.time < nextHitAt) return;
        nextHitAt = Time.time + hitCooldown;

        if (latchOn && isOn) return;
        SetOn(latchOn ? true : !isOn);
    }

    private void ApplyVisuals()
    {
        if (!spriteRenderer) return;
        Sprite target = isOn ? onSprite : offSprite;
        if (target) spriteRenderer.sprite = target;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetOn(bool _on)
    {
        if (isOn == _on) return;
        isOn = _on;

        ApplyVisuals();
        output.Set(isOn);
        if (isOn) onTurnedOn.Invoke();
        else onTurnedOff.Invoke();
    }

    #endregion
}
