//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using RivenFramework;
using UnityEngine;

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
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    private new DSPawnActions action = new DSPawnActions();
    private InputActions.DepthScrollerActions inputActions;
    [SerializeField] private GameObject DeathScreenWidget;
    [SerializeField] private Pawn_Inventory playerInventory;
    private ApplicationSettings applicationSettings;
    public Animator animator;
    
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
        
        // Setup inputs
        inputActions = new InputActions().DepthScroller;
        inputActions.Enable();
        
        // Enable the view camera
        //action.EnableViewCamera(this, true);
        animator = GetComponentInChildren<Animator>();
    }

    public void Update()
    {
        // Pausing
        UpdatePauseMenu();
        if (HasControl is false) return;
        
        // Kill bind
        if (Input.GetKeyDown(KeyCode.Delete)) Kill();
        
        // Basic Movement
        UpdateMovement();
        UpdateRotation();
        
        // Crouching
        action.Crouch(this, inputActions.Fall.IsPressed());

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

        if (action.isJumping && !action.isFastFalling) animator.Play("Jumping");
        if (physicsbody.velocity.y < 0 && !action.IsOnGround(this))
        {
            if (!action.isFastFalling)
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

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private bool HasControl => !isPaused && !isDead;
    
    private void UpdateMovement()
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, 0, inputActions.Move.ReadValue<Vector2>().y);
        animator.SetFloat("MoveX", moveDirection.x);
    }
    private void ApplyMovement()
    {
        action.Move(this, moveDirection);
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
        Destroy(widgetManager.GetExistingWidget("WB_HUD"));
        // Add the respawn HUD
        widgetManager.AddWidget(DeathScreenWidget);

        // Play the death animation
        if (TryGetComponent(out Animator animator)) animator.Play("Death");
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/


    #endregion
}
