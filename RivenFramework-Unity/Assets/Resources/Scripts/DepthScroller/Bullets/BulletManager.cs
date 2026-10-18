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
using RivenFramework;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public SpriteRenderer bulletPrefab;
    public float playerHitRadius = 0.15f;
    public Vector2 playerHitOffset = new Vector2(0, 0.9f);
    public float invulnerabilityTime = 0.75f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private readonly Bullet[] bullets = new Bullet[4096];
    private int count;
    private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
    private Zone zone;
    private Pawn player;
    private float invulnerableUntil;
    private Zone lastZone;
    
    private Color[] layerTints = new Color[0];
    private Color baseColor = Color.white;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public static BulletManager Instance;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    void Awake()
    {
        Instance = this;
        if (bulletPrefab) baseColor = bulletPrefab.color;
    }
    private void Update()
    {
        if (!zone) zone = FindObjectOfType<Zone>();
        if (zone != lastZone) { ClearAll(); lastZone = zone; }
        if (!zone) return;
        
        if (!player)
        {
            var pawnManager = FindObjectOfType<GI_PawnManager>();
            if (pawnManager && pawnManager.localPlayerCharacter) player = pawnManager.localPlayerCharacter.GetComponent<Pawn>();
        }
        
        int hitLayer = zone.currentDepth;
        bool canHit = player && Time.time >= invulnerableUntil && hitLayer < zone.depthLayers.Count;
        Vector2 localPlayer = canHit ? LocalPlayer(hitLayer) : default;
        float deltaTime = Time.deltaTime;
        
        for (int i = 0; i < count; i++)
        {
            ref Bullet bullet = ref bullets[i];
            bullet.life -= deltaTime;
            if (bullet.turnRate != 0f) bullet.velocity = Quaternion.Euler(0, 0, bullet.turnRate * deltaTime) * bullet.velocity;
            if (bullet.acceleration != 0f) bullet.velocity += bullet.velocity.normalized * (bullet.acceleration * deltaTime);
            bullet.position += bullet.velocity * deltaTime;

            bool kill = bullet.life <= 0f;
            if (!kill && canHit && bullet.layer == hitLayer)
            {
                float r = bullet.radius + playerHitRadius;
                if ((bullet.position - localPlayer).sqrMagnitude <= r * r)
                {
                    player.ModifyHealth(-bullet.damage);
                    invulnerableUntil = Time.time + invulnerabilityTime;
                    canHit = false;
                    kill = true;
                }
            }

            if (kill)
            {
                bullet.renderer.gameObject.SetActive(false);
                pool.Push(bullet.renderer);
                bullets[i] = bullets[--count];
                i--;
            }
        }
    }
    
    private void LateUpdate()
    {
        if (!zone) return;

        int layerCount = zone.depthLayers.Count;
        if (layerTints.Length != layerCount) layerTints = new Color[layerCount];
        for (int i = 0; i < layerCount; i++) layerTints[i] = zone.GetSpriteTint(i) * baseColor;

        for (int i = 0; i < count; i++)
        {
            ref Bullet bullet = ref bullets[i];
            var layerTransform = zone.depthLayers[bullet.layer].transform;
            bool visible = layerTransform.gameObject.activeInHierarchy;
            bullet.renderer.gameObject.SetActive(visible);
            if (!visible) continue;

            bullet.renderer.transform.position = layerTransform.TransformPoint(bullet.position);
            bullet.renderer.color = layerTints[bullet.layer];
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private Vector2 LocalPlayer(int _layer)
    {
        return zone.depthLayers[_layer].transform.InverseTransformPoint(player.transform.position + (Vector3)playerHitOffset);
    }

    private float AngleToPlayer(int _layer, Vector2 _origin)
    {
        if (!player || !zone) return -90f;
        Vector2 d = LocalPlayer(_layer) - _origin;
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
    }
    
    public void ClearAll()
    {
        for (int i = 0; i < count; i++)
        {
            bullets[i].renderer.gameObject.SetActive(false);
            pool.Push(bullets[i].renderer);
        }
        count = 0;
    }

    private void OnDrawGizmos()
    {
        if (player) Gizmos.DrawWireSphere(player.transform.position+new Vector3(playerHitOffset.x, playerHitOffset.y, 0), playerHitRadius);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Fire(BulletPattern _pattern, int _layer, Vector2 _origin, float _extraAngle = 0f)
    {
        float baseAngle = _pattern.angleOffset + _extraAngle;
        if (_pattern.aimAtPlayer) baseAngle += AngleToPlayer(_layer, _origin);

        bool ring = _pattern.arc >= 360f;
        float step = _pattern.count <= 1 ? 0f : (ring ? 360f / _pattern.count : _pattern.arc / (_pattern.count - 1));
        float start = ring || _pattern.count <= 1 ? baseAngle : baseAngle - _pattern.arc * 0.5f;

        for (int i = 0; i < _pattern.count; i++)
        {
            Spawn(_pattern, _layer, _origin, start + step * i);
        }
    }

    public void Spawn(BulletPattern _pattern, int _layer, Vector2 _position, float angleDeg)
    {
        if (count >= bullets.Length) return;
        var renderer = pool.Count > 0 ? pool.Pop() : Instantiate(bulletPrefab, transform);
        float angle = angleDeg * Mathf.Deg2Rad;
        bullets[count++] = new Bullet
        {
            position = _position,
            velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _pattern.speed,
            acceleration = _pattern.acceleration,
            turnRate = _pattern.turnRate,
            life = _pattern.lifetime,
            radius = _pattern.radius,
            damage = _pattern.damage,
            layer = _layer,
            renderer = renderer
        };
    }
    
    


    #endregion
}

public struct Bullet
{
    public Vector2 position, velocity;
    public float acceleration, turnRate, life, radius, damage;
    public int layer;
    public SpriteRenderer renderer;
}
