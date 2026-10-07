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
using UnityEngine.EventSystems;

public class WB_CursorZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Texture2D customCursor;
    public enum CursorAppearance
    {
        System,
        Custom,
    }


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void OnPointerEnter(PointerEventData _eventData) => SetCustomAppearance(CursorAppearance.Custom);
    public void OnPointerExit(PointerEventData _eventData) => SetCustomAppearance(CursorAppearance.System);
    
    public void SetCustomAppearance(CursorAppearance _appearance)
    {
        switch (_appearance)
        {
            case CursorAppearance.System:
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                break;
            case CursorAppearance.Custom:
                Cursor.SetCursor(customCursor, hotspot, cursorMode);
                break;
        }
    }
    private void OnDisable()
    {
        SetCustomAppearance(CursorAppearance.Custom);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
