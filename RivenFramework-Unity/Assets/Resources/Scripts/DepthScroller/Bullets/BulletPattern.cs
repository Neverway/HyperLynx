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
using UnityEngine;

[CreateAssetMenu(menuName = "HyLi/BulletPattern")]
public class BulletPattern : ScriptableObject
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("How many bullets to spawn for this burst")]
    public int count = 12;
    [Tooltip("How many degrees to fire the bullet burst")]
    public float arc = 360f;
    [Tooltip("")]
    public float angleOffset;
    [Tooltip("If enabled, the burst starts aimed at the player")]
    public bool aimAtPlayer;
    [Tooltip("How fast each bullet moves")]
    public float speed = 6f;
    [Tooltip("How fast each bullet increases its speed")]
    public float acceleration;
    [Tooltip("Degrees per second the bullet curves")]
    public float turnRate;
    [Tooltip("How long each bullet exists for")]
    public float lifetime = 6f;
    [Tooltip("How big the bullet is")]
    public float radius = 0.15f;
    [Tooltip("How much pain the bullet causes to the player")]
    public float damage = 10f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
