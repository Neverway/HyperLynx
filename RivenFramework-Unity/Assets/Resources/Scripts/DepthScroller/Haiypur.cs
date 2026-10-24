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

public class Haiypur : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public AnimationStates animationState;
    public Transform lookTarget;
    public bool targetIsPlayer;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Animator animator;
    public GameObject visuals;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/

    private void Start()
    {
        UpdateAnimationState();
    }

    private void Update()
    {
        GetSideToTarget();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public void GetSideToTarget()
    {
        if (targetIsPlayer && !lookTarget)
        {
            lookTarget = FindObjectOfType<DSPawn_Player>().gameObject.transform;
        }
        if (!lookTarget)
        {
            visuals.transform.localScale = Vector3.one;
            return;
        }
        Vector3 localPos = transform.InverseTransformPoint(lookTarget.position);
        
        if (localPos.x > 0) 
        {
            // Target is to the left
            visuals.transform.localScale = new Vector3(-1, 1, 1);
        } 
        else if (localPos.x < 0) 
        {
            // Target is to the right
            visuals.transform.localScale = Vector3.one;
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void UpdateAnimationState()
    {
        switch (animationState)
        {
            case AnimationStates.Idle:
                animator.Play("Haiypur_Idle");
                break;
            case AnimationStates.Exclaiming:
                animator.Play("Haiypur_Exclaiming");
                break;
            case AnimationStates.Thinking:
                animator.Play("Haiypur_Thinking");
                break;
            case AnimationStates.Pointing:
                animator.Play("Haiypur_Pointing");
                break;
        }
    }


    #endregion
}

public enum AnimationStates
{
    Idle,
    Exclaiming,
    Thinking,
    Pointing,
}
