//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_HUD : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Texture2D customCursor;
    [SerializeField] private Image customCursorZone;
    public enum CursorAppearance
    {
        System,
        Custom,
    }
    
    [SerializeField] private Image cpuBar, ramBar;
    [SerializeField] private TMP_Text cpuText, ramText;
    private DSPawn_Player player;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        SetCustomAppearance(CursorAppearance.Custom);
        player = FindObjectOfType<DSPawn_Player>();
    }

    private void Update()
    {
        if (!player)
        {
            player = FindObjectOfType<DSPawn_Player>();
            return;
        }
        cpuText.text = $"{player.currentStats.health}%";
        cpuBar.fillAmount = (player.currentStats.health / player.defaultStats.health) * 100;
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
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


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    public void TogglePause()
    {
        var widgetManager = FindObjectOfType<GI_WidgetManager>();
        widgetManager.ToggleWidget("WB_Pause");
    }


    #endregion
}
