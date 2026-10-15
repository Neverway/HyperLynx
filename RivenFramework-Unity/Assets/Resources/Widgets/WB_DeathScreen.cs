//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WB_DeathScreen : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private bool acceptingInputs;
    private Coroutine crashTextCo;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_WorldLoader worldLoader;
    private InputActions.DepthScrollerActions inputActions;
    public TMP_Text textContent;
    [TextArea]public string[] crashTextSequence;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        // Setup inputs
        inputActions = new InputActions().DepthScroller;
        inputActions.Enable();
        
        StartCoroutine(InputDelay());

        crashTextCo = StartCoroutine(ShowCrashText());
    }

    private void Update()
    {
        if (!acceptingInputs) return;
        if (inputActions.Menu.WasPressedThisFrame())
        {
            if (crashTextCo != null)
            {
                SkipCrashText();
            }
            Application.Quit();
        }
        if (inputActions.JumpAndKick.WasPressedThisFrame())
        {
            worldLoader = FindObjectOfType<GI_WorldLoader>();
            worldLoader.ForceLoadWorld(SceneManager.GetActiveScene().name);
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private IEnumerator InputDelay()
    {
        yield return new WaitForSeconds(0.2f);
        acceptingInputs = true;
    }

    private IEnumerator ShowCrashText()
    {
        textContent.text = "";
        var delay = 0.125f;
        for (int i = 0; i < crashTextSequence.Length; i++)
        {
            textContent.text = crashTextSequence[i];
            yield return new WaitForSeconds(delay);
        }

        crashTextCo = null;
    }

    private void SkipCrashText()
    {
        StopCoroutine(crashTextCo);
        textContent.text = crashTextSequence[^1];
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
