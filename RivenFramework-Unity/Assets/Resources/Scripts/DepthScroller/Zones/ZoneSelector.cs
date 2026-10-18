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
using DG.Tweening;
using RivenFramework;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class ZoneSelector : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Color colorHovered;
    public Color colorClicked;
    public Color colorRegular = Color.white;
    public float doubleClickTime = 0.3f;
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private float lastClickTime = -999f;
    private bool isHovered;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private ZoneManager zoneManager;
    public GameObject targetZone;
    public SpriteRenderer selectionSprite;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnMouseEnter()
    {
        isHovered = true;
        selectionSprite.DOColor(colorHovered, 0.25f);
    }
    
    private void OnMouseExit()
    {
        isHovered = false;
        selectionSprite.DOColor(colorRegular, 0.25f);
    }
    
    private void OnMouseUp()
    {
        selectionSprite.DOColor(isHovered ? colorHovered : colorRegular, 0.25f);
    }

    private void OnMouseDown()
    {
        selectionSprite.DOColor(colorClicked, 0.25f);

        if (Time.unscaledTime - lastClickTime > doubleClickTime)
        {
            lastClickTime = Time.unscaledTime;
            return;
        }

        lastClickTime = -999f;
        OpenZone();
    }
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void OpenZone()
    {
        if (targetZone == null)
        {
            Debug.LogWarning("No target zone has been set for this zone selector, please assign one!");
            return;
        }
        
        if (!zoneManager)
        {
            zoneManager = GameInstance.Get<ZoneManager>();
        }

        var target = zoneManager.GetZoneEntry(targetZone);

        if (target == null)
        {
            Debug.LogWarning($"Target zone '{targetZone}' could not be found on the zone manager! Please ensure you created a zone entry on the Game Instance's Zone manager for this target zone!");
            return;
        }
        
        if (target.zoneState != ZoneState.Unlocked) return;
        zoneManager.LoadZone(targetZone);
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
