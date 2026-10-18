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
using Neverway.Framework.PawnManagement;
using UnityEngine;
using GameInstance = RivenFramework.GameInstance;

public class Attack_AimedBurst : EnemyAttack
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public BulletPattern pattern;
    public float initialDelay = 1.5f;
    public int shotsPerBurst = 3;
    public float shotInterval = 0.15f;
    public float burstInterval = 2f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public override IEnumerator Run(DSPawn_Enemy _enemy)
    {
        var shotWait = new WaitForSeconds(shotInterval);
        var burstWait = new WaitForSeconds(burstInterval);

        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            for (int i = 0; i < shotsPerBurst; i++)
            {
                GameInstance.Get<BulletManager>().Fire(pattern, _enemy.depth, _enemy.LocalPosition);
                yield return shotWait;
            }

            yield return burstWait;
        }
    }

    #endregion
}
