//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RivenFramework;
using UnityEngine;
using UnityEngine.InputSystem;

public class DSPawn_Player : DSPawn
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private Vector3 moveDirection;
    private Vector2 lookRotation;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;

    private Tween flashTween;
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    [SerializeField] private new DSPawnActions action = new DSPawnActions();
    private InputActions.DepthScrollerActions inputActions;
    [SerializeField] private GameObject DeathScreenWidget;
    [SerializeField] private Pawn_Inventory playerInventory;
    private ApplicationSettings applicationSettings;

    public Camera viewCamera;

    public Zone zone;
    private ZoneManager zonemanager;
    public GameObject characterVisuals;
    public ParticleSystem hurtParticleEmitter;

    private List<SpriteRenderer> spritesRenderers;
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private void UpdatePauseMenu()
    {
        if (!widgetManager)
        {
            widgetManager = GameInstance.Get<GI_WidgetManager>();
            if (!widgetManager) return;
        }
        isPaused = widgetManager.GetExistingWidget("WB_Pause");
        
        // Pause Game
        if (inputActions.Menu.WasPressedThisFrame())
        {
            widgetManager.ToggleWidget("WB_Pause");
        }
        
        // Lock mouse when unpaused, unlock when paused
        /*if (isPaused)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }*/

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public new void Awake()
    {
        base.Awake();
        
        // Subscribe to events
        OnPawnDeath += OnDeath;
        OnPawnHurt += OnHurt;

        // Setup inputs
        inputActions = new InputActions().DepthScroller;
        inputActions.Enable();
        
        // Enable the view camera
        //action.EnableViewCamera(this, true);

        spritesRenderers = GetComponentsInChildren<SpriteRenderer>(true).ToList();
        foreach (var r in spritesRenderers) originalColors[r] = r.color;
        
        StartCoroutine(DoLevelSubCrap());
    }

    private IEnumerator DoLevelSubCrap()
    {
        yield return new WaitForEndOfFrame();
        zonemanager = FindObjectOfType<ZoneManager>();
        if (zonemanager)
        {
            FindObjectOfType<ZoneManager>().OnZoneChanged -= OnOnZoneChanged;
            FindObjectOfType<ZoneManager>().OnZoneChanged += OnOnZoneChanged;
        }
    }

    public void Update()
    {
        // Pausing
        UpdatePauseMenu();
        if (HasControl is false)
        {
            if (action.isKicking || action.isChargingSuperKick) action.CancelKicks(this);
            return;
        }
        
        // Kill bind
        if (Input.GetKeyDown(KeyCode.Delete)) Kill();
        
        // Basic Movement
        UpdateMovement();
        UpdateRotation();
        UpdateMouseSide();
        
        // Crouching
        action.Crouch(this, inputActions.Fall.IsPressed());

        // Kicking
        if (inputActions.LeftAction.WasPressedThisFrame() && !action.isKicking)
        {
            action.Kick(this);
        }

        if (inputActions.RightAction.IsPressed() && !action.isChargingSuperKick && !action.isKicking)
        {
            action.SuperKickStart(this);
        }

        if (!inputActions.RightAction.IsPressed() && action.isChargingSuperKick)
        {
            action.SuperKickRelease(this);
        }

        // Jumping
        action.jumpHeld = inputActions.JumpAndKick.IsPressed();
        if (inputActions.JumpAndKick.WasPressedThisFrame()) action.Jump(this);
    }

    public void FixedUpdate()
    {
        if (HasControl is false) return;
        
        action.UpdateCoyoteTime(this);
        action.ApplyJumpGravity(this);
        action.ApplyFastFall(this, inputActions.Fall.IsPressed());
        
        ApplyMovement();
        ApplyRotation();

        if (action.isJumping && !action.isFastFalling && !action.isKicking && !action.isChargingSuperKick) animator.Play("Jumping");
        if (physicsbody.velocity.y < 0 && !action.IsOnGround(this))
        {
            if (!action.isFastFalling && !action.isKicking && !action.isChargingSuperKick)
            {
                animator.Play("Falling");
                physicsbody.velocity += Vector3.up * Physics.gravity.y * DSCurrentStats.fallForce * Time.fixedDeltaTime;
            }
            if (action.isFastFalling)
            {
                animator.Play("FastFalling");
            }
        }
    }

    private bool GetActiveZone()
    {
        if (zone)
        {
            if (zone.isActiveAndEnabled is false) zone = null;
        }
        if (zone == null)
        {
            zone = FindObjectOfType<Zone>(false);
        }

        return zone != null;
    }

    private void OnDestroy()
    {
        if (zonemanager) zonemanager.OnZoneChanged -= OnOnZoneChanged;
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private bool HasControl => !isPaused && !isDead;
    
    private void UpdateMovement()
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, 0, inputActions.Move.ReadValue<Vector2>().y);


        if (inputActions.StepForwards.WasPressedThisFrame())
        {
            if (GetActiveZone()) zone.StepDepth(+1);
        }

        if (inputActions.StepBackwards.WasPressedThisFrame())
        {
            if (GetActiveZone()) zone.StepDepth(-1);
        }
        
        animator.SetFloat("MoveX", moveDirection.x);
    }
    private void ApplyMovement()
    {
        action.Move(this, moveDirection);
    }

    private void OnHurt(DamageInfo info = default)
    {
        characterVisuals.transform.DOShakePosition(0.125f, new Vector3(0.05f, 0, 0));
        hurtParticleEmitter.Play();

        foreach (var renderer in spritesRenderers)
        {
            Flash(renderer);
        }
    }

    private void UpdateRotation()
    {/*
        if (applicationSettings == null) applicationSettings = GameInstance.Get<ApplicationSettings>();
        
        // Get the look speed
        float horizontalLookSpeed = applicationSettings.currentSettingsData.horizontalLookSpeed;
        float verticalLookSpeed = applicationSettings.currentSettingsData.verticalLookSpeed;
        
        // Separate multipliers for mouse and joystick
        float mouseMultiplier = applicationSettings.currentSettingsData.mouseLookSensitivity;
        float joystickMultiplier = applicationSettings.currentSettingsData.joystickLookSensitivity;

        // Determine the input method (mouse or joystick)
        bool isUsingMouse = false;
        if (inputActions.LookAxis.IsInProgress())
        {
            if (inputActions.LookAxis.activeControl.device.name == "Mouse")
            {
                isUsingMouse = true;
            }
        }

        // Apply the appropriate multiplier
        var multiplier = isUsingMouse ? mouseMultiplier : joystickMultiplier;
        
        // Store the rotation values
        lookRotation.x -= inputActions.LookAxis.ReadValue<Vector2>().y * (10 * verticalLookSpeed) * (multiplier/10);
        lookRotation.y += inputActions.LookAxis.ReadValue<Vector2>().x * (10 * horizontalLookSpeed) * (multiplier/10);
        lookRotation.x = Mathf.Clamp(lookRotation.x, -90f, 90f);*/
    }
    private void ApplyRotation()
    {
        action.FaceTowardsDirection(this, viewPoint, lookRotation);
    }

    private void OnDeath(DamageInfo _damageInfo)
    {
        // Remove the HUD
        //Destroy(widgetManager.GetExistingWidget("WB_HUD"));
        // Add the respawn HUD
        widgetManager.AddWidget(DeathScreenWidget);
        
        GameInstance.Get<MusicList>().CrossFadeToTrack(MusicTracks.GameOver, 0.1f);

        // Play the death animation
        if (TryGetComponent(out Animator animator)) animator.Play("Death");
        
        action.CancelKicks(this);
    }

    private void OnOnZoneChanged()
    {
        isPaused = true;
        action.CancelKicks(this);
        StartCoroutine(ChangeZonesCo());
    }

    private IEnumerator ChangeZonesCo()
    {
        physicsbody.isKinematic = true;
        gameObject.transform.DOLocalMove(new Vector3(0, -3, 0), 0.25f);
        animator.Play("Falling");
        yield return new WaitForSeconds(0.25f);
        isPaused = false;
        physicsbody.isKinematic = false;
    }
    
    
    public void Flash(SpriteRenderer targetRenderer)
    {
        targetRenderer.DOKill();

        Color original = originalColors[targetRenderer];
        targetRenderer.color = original;

        targetRenderer.DOColor(Color.red, flashDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetLink(targetRenderer.gameObject);
    }

    private void UpdateMouseSide()
    {
        if (Mouse.current == null) return;
        if (!viewCamera) viewCamera = FindObjectOfType<Camera>();
        if (!viewCamera) return;

        float playerX = viewCamera.WorldToScreenPoint(transform.position).x;
        float mouseX = Mouse.current.position.ReadValue().x;
        float delta = mouseX - playerX;

        var mouseDeadzonePixels = 10f;
        //float side = Mathf.Clamp(delta / (Screen.width * 0.25f), -1f, 1f);
        float side = Mathf.Abs(delta) < mouseDeadzonePixels ? 0f : Mathf.Sign(delta);
        
        animator.SetFloat("MouseX", side);
        animator.SetFloat("IdleX", side);
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/


    #endregion
}
