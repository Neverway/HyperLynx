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

public class Attack_Spiral : EnemyAttack
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public BulletPattern pattern;
    public int shots = 40;
    public float interval = 0.08f;
    public float twistPerShot = 11f;
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
        while (true)
        {
            for (int i = 0; i < shots; i++)
            {
                yield return new WaitUntil(() => !_enemy.IsStunned);
                GameInstance.Get<BulletManager>().Fire(pattern, _enemy.depth, _enemy.LocalPosition, twistPerShot * i);
                yield return new WaitForSeconds(interval);
            }
            yield return new WaitForSeconds(restTime);
        }
    }

    #endregion
}
