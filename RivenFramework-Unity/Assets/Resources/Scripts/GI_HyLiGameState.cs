//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using RivenFramework;
using UnityEngine;

public class GI_HyLiGameState : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public const string GameStateID = "HyLiGameState";
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Box] public GameStateSaveData saveData;

    [Serializable]
    public class GameStateSaveData
    {
        public float playtime = 0;
        public float health = 100;
        public int cache = 0;
        public int deaths = 0;
        public float damageTaken = 0;
        public float damageDealt = 0;
    }


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    [InvokeAfterLoad(int.MaxValue - 100)]
    public static void OnGameLoad() => 
        GI_HyLiGameState.GetInstance().saveData = GI_SaveSystem.LoadValue<GameStateSaveData>(new GameStateSaveData(), GI_HyLiGameState.GameStateID);

    [InvokeBeforeSave(int.MaxValue - 100)]
    public static void OnGameSave() => 
        GI_SaveSystem.SaveValue(GI_HyLiGameState.GetInstance().saveData, GI_HyLiGameState.GameStateID);

    [InvokeBeforeNewGame(int.MaxValue - 100)]
    public static void OnNewGame() => 
        GI_HyLiGameState.GetInstance().saveData = new GameStateSaveData();

    public static GI_HyLiGameState GetInstance() => 
        GameInstance.Get<GI_HyLiGameState>();


    #endregion
}