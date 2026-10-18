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

public class Move_EnterHoverExit : EnemyMovement
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector2 hoverOffset = new Vector2(0f, -4f);
    public float enterTime = 1.5f;

    public float hoverTime = 5f;

    public Vector2 exitDirection = Vector2.up;
    public float exitSpeed = 2f;
    public float exitAcceleration = 8f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public float TimeUntilExit()
    {
        return enterTime + hoverTime;
    }

    public override Vector2 Offset(float t)
    {
        if (t < enterTime)
        {
            float u = enterTime <= 0f ? 1f : t / enterTime;
            float eased = 1f - Mathf.Pow(1f - u, 3f);
            return hoverOffset * eased;
        }

        t -= enterTime;
        if (t < hoverTime) return hoverOffset;

        float s = t - hoverTime;
        return hoverOffset + exitDirection.normalized * (exitSpeed * s + 0.5f * exitAcceleration * s * s);
    }


    #endregion
}
