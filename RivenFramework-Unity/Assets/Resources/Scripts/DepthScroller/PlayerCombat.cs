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

public class PlayerCombat : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Kick")]
    public AttackProfile kick = new AttackProfile
    {
        boxSize = new Vector2(1.2f, 1f),
        boxOffset = new Vector2(0.9f, 0.9f),
        hitDelay = 0.08f,
        hitDuration = 0.12f,
        damageMin = 10f,
        damageMax = 10f,
        knockbackMin = 1.5f,
        knockbackMax = 1.5f,
        knockbackUp = 0.25f,
        recoilMin = 0f,
        recoilMax = 0f
    };
 
    [Header("Super Kick")]
    public AttackProfile superKick = new AttackProfile
    {
        boxSize = new Vector2(1.8f, 1.4f),
        boxOffset = new Vector2(1.1f, 0.9f),
        hitDelay = 0.05f,
        hitDuration = 0.15f,
        damageMin = 20f,
        damageMax = 60f,
        knockbackMin = 3f,
        knockbackMax = 9f,
        knockbackUp = 0.5f,
        recoilMin = 6f,
        recoilMax = 14f
    };
 
    [Header("Stomp")]
    public float stompDamage = 25f;
    public float stompBounceVelocity = 14f;
    public float feetHalfWidth = 0.3f;
    public float stompTolerance = 0.15f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Zone zone;
    private DSPawn_Player player;
    private DSPawnActions actions;
    private Rigidbody physicsbody;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        player = GetComponent<DSPawn_Player>();
        physicsbody = GetComponent<Rigidbody>();
        actions = player.CombatActions;
 
        actions.OnKick += HandleKick;
        actions.OnSuperKick += HandleSuperKick;
    }
 
    private void OnDestroy()
    {
        if (actions == null) return;
        actions.OnKick -= HandleKick;
        actions.OnSuperKick -= HandleSuperKick;
    }
 
    private void FixedUpdate()
    {
        UpdateStomp();
    }
 
    private void OnDrawGizmosSelected()
    {
        float facing = player ? player.facing : 1f;
        DrawBox(kick, facing, Color.yellow);
        DrawBox(superKick, facing, new Color(1f, 0.5f, 0f));
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void DrawBox(AttackProfile _profile, float _facing, Color _color)
    {
        Gizmos.color = _color;
        var center = transform.position + new Vector3(_profile.boxOffset.x * _facing, _profile.boxOffset.y, 0f);
        Gizmos.DrawWireCube(center, _profile.boxSize);
    }
 
    private bool GetCurrentLayer(out Transform _layer)
    {
        _layer = null;
        if (!zone) zone = FindObjectOfType<Zone>();
        if (!zone) return false;
        if (zone.currentDepth < 0 || zone.currentDepth >= zone.depthLayers.Count) return false;
 
        _layer = zone.depthLayers[zone.currentDepth].transform;
        return _layer != null;
    }
 
    private void HandleKick()
    {
        StartCoroutine(HitWindow(kick, 0f, false));
    }
 
    private void HandleSuperKick(float _power)
    {
        StartCoroutine(HitWindow(superKick, _power, true));
    }
 
    private IEnumerator HitWindow(AttackProfile _profile, float _power, bool _isSuper)
    {
        float facing = player.facing;
 
        float damage = Mathf.Lerp(_profile.damageMin, _profile.damageMax, _power);
        float knockback = Mathf.Lerp(_profile.knockbackMin, _profile.knockbackMax, _power);
        float recoil = Mathf.Lerp(_profile.recoilMin, _profile.recoilMax, _power);
 
        if (_profile.hitDelay > 0f) yield return new WaitForSeconds(_profile.hitDelay);
 
        float endTime = Time.time + _profile.hitDuration;
        var alreadyHit = new HashSet<EnemyCombatReceiver>();
        bool recoiled = false;
 
        while (Time.time < endTime)
        {
            if (GetCurrentLayer(out var layer))
            {
                Vector2 playerLocal = layer.InverseTransformPoint(transform.position);
                Vector2 center = playerLocal + new Vector2(_profile.boxOffset.x * facing, _profile.boxOffset.y);
                var box = new Rect(center - _profile.boxSize * 0.5f, _profile.boxSize);
 
                for (int i = EnemyCombatReceiver.Active.Count - 1; i >= 0; i--)
                {
                    var enemy = EnemyCombatReceiver.Active[i];
                    if (!enemy || enemy.Depth != zone.currentDepth || alreadyHit.Contains(enemy)) continue;
                    if (!box.Overlaps(enemy.Bounds)) continue;
 
                    alreadyHit.Add(enemy);
                    enemy.TakeKick(damage, new Vector2(facing * knockback, _profile.knockbackUp));
 
                    if (_isSuper && !recoiled)
                    {
                        recoiled = true;
                        physicsbody.velocity = new Vector3(-facing * recoil, physicsbody.velocity.y, physicsbody.velocity.z);
                    }
                }
            }
 
            yield return new WaitForFixedUpdate();
        } 
    }
 
    private void UpdateStomp()
    {
        if (actions == null || !actions.isFastFalling) return;
        if (physicsbody.velocity.y >= 0f) return;
        if (!GetCurrentLayer(out var layer)) return;
 
        Vector2 feet = layer.InverseTransformPoint(transform.position);
 
        float reach = -physicsbody.velocity.y * Time.fixedDeltaTime + stompTolerance;
 
        for (int i = EnemyCombatReceiver.Active.Count - 1; i >= 0; i--)
        {
            var enemy = EnemyCombatReceiver.Active[i];
            if (!enemy || enemy.Depth != zone.currentDepth || !enemy.CanBeStomped) continue;
 
            float halfWidth = enemy.hitboxSize.x * 0.5f + feetHalfWidth;
            if (Mathf.Abs(feet.x - enemy.Center.x) > halfWidth) continue;
 
            float top = enemy.Top;
            if (feet.y > top + stompTolerance || feet.y < top - reach) continue;
 
            enemy.TakeStomp(stompDamage);
            actions.Bounce(player, stompBounceVelocity);
            return;
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}

[Serializable]
public class AttackProfile
{
    [Header("Hitbox")]
    public Vector2 boxSize = new Vector2(1f, 1f);
    public Vector2 boxOffset = new Vector2(1f, 0.9f);
 
    [Header("Timing")]
    public float hitDelay = 0.05f;
    public float hitDuration = 0.12f;
 
    [Header("Damage")]
    public float damageMin = 10f;
    public float damageMax = 10f;
 
    [Header("Enemy Knockback")]
    public float knockbackMin = 1.5f;
    public float knockbackMax = 1.5f;
    public float knockbackUp = 0.25f;
 
    [Header("Player Recoil")]
    public float recoilMin = 0f;
    public float recoilMax = 0f;
}
