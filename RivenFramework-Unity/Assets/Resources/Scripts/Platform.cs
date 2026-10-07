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

public class Platform : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private Collider solid;
    [SerializeField] private float footTolerance = 0.1f;

    private readonly HashSet<Collider> dropping = new HashSet<Collider>();



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnTriggerEnter(Collider other) => Evaluate(other);
    
    private void OnTriggerStay(Collider other) => Evaluate(other);
    
    private void OnTriggerExit(Collider other)
    {
        dropping.Remove(other);
        Physics.IgnoreCollision(other, solid, false);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void Evaluate(Collider other)
    {
        if (other.isTrigger) return;
        
        var player = other.GetComponentInParent<DSPawn_Player>();
        if (player == null) return;

        if (dropping.Contains(other)) return;

        bool feetAbove = other.bounds.min.y >= solid.bounds.max.y - footTolerance;

        if (feetAbove && player.WantsToDrop)
        {
            dropping.Add(other);
            Physics.IgnoreCollision(other, solid, true);
            return;
        }
        
        Physics.IgnoreCollision(other, solid, !feetAbove);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
