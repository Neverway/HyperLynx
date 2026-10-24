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
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleChoicebox : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_Text query;
    public TMP_Text choiceTextA, choiceTextB, choiceTextC;
    public Button choiceButtonA, choiceButtonB, choiceButtonC;
    public UnityEvent OnChoiceA, OnChoiceB, OnChoiceC;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetOptions(string _choiceA, string _choiceB, string _choiceC)
    {
        choiceButtonA.gameObject.SetActive(_choiceA != "");
        choiceButtonB.gameObject.SetActive(_choiceB != "");
        choiceButtonC.gameObject.SetActive(_choiceC != "");
        choiceTextA.text = _choiceA;
        choiceTextB.text = _choiceB;
        choiceTextC.text = _choiceC;
    }

    public void SetEvents(UnityEvent _OnChoiceA, UnityEvent _OnChoiceB, UnityEvent _OnChoiceC)
    {
        OnChoiceA = _OnChoiceA;
        OnChoiceB = _OnChoiceB;
        OnChoiceC = _OnChoiceC;
    }

    // Called by the buttons
    public void SelectChoice(int _choice)
    {
        switch (_choice)
        {
            case 0:
                OnChoiceA?.Invoke();
                break;
            case 1:
                OnChoiceB?.Invoke();
                break;
            case 2:
                OnChoiceC?.Invoke();
                break;
        }
        Destroy(gameObject);
    }


    #endregion
}
