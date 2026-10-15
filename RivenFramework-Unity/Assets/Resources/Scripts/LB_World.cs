//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System.Collections;
using RivenFramework;
using UnityEngine;

/// <summary>
///  This is a Level Blueprint (LB) script, it is attached to the WorldSettings
///  object in a scene.
///  This LB makes the HUD widget appear on game maps.
/// </summary>
public class LB_World : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    // TODO: This may be better changed from GameObject to reference a parent WB_HUD class
    [Tooltip("A reference to the HUD widget prefab to draw to the UI")]
    [SerializeField] private GameObject HUDWidgetPrefab;

    public GameObject startingZone;
    
    #endregion
    
    
    #region=======================================( Functions )=======================================================//
    
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private IEnumerator Start()
    {
        widgetManager = GameInstance.Get<GI_WidgetManager>();
        Debug.Log(widgetManager);
        widgetManager.AddWidget(HUDWidgetPrefab);
        yield return new WaitForEndOfFrame();
        
        GameInstance.Get<ZoneManager>().LoadZone(startingZone);
    }
    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    
    
    #endregion
    
}