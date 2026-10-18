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

public class Move_Circle : EnemyMovement
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public float radius = 2f;
    public float angularSpeed = 90f;
    public float startAngle = 0f;
    public bool clockwise;
    public float radiusGrowth;
    public Vector2 drift;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public override Vector2 Offset(float t)
    {
        float startRadians = startAngle * Mathf.Deg2Rad;
        float direction = clockwise ? -1f : 1f;
        float angle = startRadians + direction * angularSpeed * Mathf.Deg2Rad * t;
        float currentRadius = Mathf.Max(0f, radius + radiusGrowth * t);

        Vector2 startPoint = radius * new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians));
        Vector2 currentPoint = currentRadius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        return currentPoint - startPoint + drift * t;
    }


    #endregion
}
