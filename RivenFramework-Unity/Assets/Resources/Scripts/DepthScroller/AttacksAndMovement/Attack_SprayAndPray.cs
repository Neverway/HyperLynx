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
using RivenFramework;
using UnityEngine;

public class Attack_SprayAndPray : EnemyAttack
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public BulletPattern pattern;
    public float initialDelay = 1.5f;
    public float sprayDuration = 3f;
    public float fireInterval = 0.06f;
    public float restTime = 1.5f;


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
        var fireWait = new WaitForSeconds(fireInterval);
        var restWait = new WaitForSeconds(restTime);

        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            float endTime = Time.time + sprayDuration;
            while (Time.time < endTime)
            {
                yield return new WaitUntil(() => !_enemy.IsStunned);
                GameInstance.Get<BulletManager>().Fire(pattern, _enemy.depth, _enemy.LocalPosition);
                yield return fireWait;
            }

            yield return restWait;
        }
    }

    #endregion
}
