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

public class CameraFollowConstrainer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Transform followTarget;
    public Vector3 followFactor, followOffset;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
    
    }

    private void Update()
    {
        if (followTarget == null)
        {
            var player = FindObjectOfType<DSPawn_Player>();
            if (player) followTarget = player.transform;
            return;
        }
        var newX = (followTarget.transform.position.x * followFactor.x) + followOffset.x;
        var newY = (followTarget.transform.position.y * followFactor.y) + followOffset.y;
        var newZ = (followTarget.transform.position.z * followFactor.z) + followOffset.z;
        transform.position = new Vector3(newX, newY, newZ);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
