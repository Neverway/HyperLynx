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

public class Attack_RingBurst : EnemyAttack
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public BulletPattern pattern;
    public float initialDelay = 1.5f;
    public float burstInterval = 1.2f;
    public bool staggerAlternateRings = true;
    public float rotatePerBurst;

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
        var burstWait = new WaitForSeconds(burstInterval);

        float halfStep = pattern.count > 0 ? 180f / pattern.count : 0f;

        yield return new WaitForSeconds(initialDelay);

        int burst = 0;
        while (true)
        {
            float extraAngle = rotatePerBurst * burst;
            if (staggerAlternateRings && burst % 2 == 1) extraAngle += halfStep;
            yield return new WaitUntil(() => !_enemy.IsStunned);
            GameInstance.Get<BulletManager>().Fire(pattern, _enemy.depth, _enemy.LocalPosition, extraAngle);
            burst++;

            yield return burstWait;
        }
    }


    #endregion
}
