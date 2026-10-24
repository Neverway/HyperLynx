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
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Events;

public class GI_TextEventHandler : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public GameObject textBoxPrefab;
    public GameObject choiceboxPrefab;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            StartCoroutine(CreateTextBoxEvent("D.D", "Wow cool, some shitty test text. This is just perfect."));
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreateChoiceBoxEvent("Do you really like this button?", "No", "Yes", "Absolutley!");
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public IEnumerator CreateTextBoxEvent(
        string _textTitle = "", 
        string _textContent = "", 
        AudioClip _audioClip = null, 
        bool _waitForAudioClipToFinish = false,
        float _textboxLifetime = 3,
        Transform _targetTransform = null,
        bool _targetIsLocalPlayer = false)
    {
        var textboxObject = Instantiate(textBoxPrefab.gameObject);
        textboxObject.transform.SetParent(GameInstance.Get<GI_WidgetManager>()._canvas.transform, true);
        textboxObject.transform.localScale = Vector3.one;
        textboxObject.transform.localPosition = Vector3.zero;
        var textbox = textboxObject.GetComponent<SimpleTextbox>();

        textbox.title.text = _textTitle;
        textbox.content.text = _textContent;
        textbox.audioSource.clip = _audioClip;
        textbox.audioSource.Play();
        if (_targetIsLocalPlayer) _targetTransform = FindObjectOfType<DSPawn_Player>().gameObject.transform;
        if (_targetTransform) textbox.FollowTarget(_targetTransform);
        
        if (_waitForAudioClipToFinish && _audioClip != null) yield return new WaitForSeconds(_audioClip.length);
        yield return new WaitForSeconds(_textboxLifetime);
        
        Destroy(textboxObject);
    }

    public SimpleChoicebox CreateChoiceBoxEvent(
        string _query = "",
        string _choiceA = "",
        string _choiceB = "",
        string _choiceC = "",
        UnityEvent _OnChoiceA = null,
        UnityEvent _OnChoiceB = null,
        UnityEvent _OnChoiceC = null,
        bool _pausesGame = false)
    {
        var choiceboxObject = Instantiate(choiceboxPrefab.gameObject);
        choiceboxObject.transform.SetParent(GameInstance.Get<GI_WidgetManager>()._canvas.transform, true);
        choiceboxObject.transform.localScale = Vector3.one;
        choiceboxObject.transform.localPosition = Vector3.zero;
        var choicebox = choiceboxObject.GetComponent<SimpleChoicebox>();

        choicebox.query.text = _query;
        choicebox.SetOptions(_choiceA, _choiceB, _choiceC);
        choicebox.SetEvents(_OnChoiceA, _OnChoiceB, _OnChoiceC);
        return choicebox;
    }


    #endregion
}
