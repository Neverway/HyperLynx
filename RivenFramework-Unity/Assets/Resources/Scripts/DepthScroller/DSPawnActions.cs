//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RivenFramework;

public class DSPawnActions : PawnActions
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private RaycastHit slopeHit;
    public bool isCrouching;
    private GameObject viewCamera;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpTime = float.NegativeInfinity;
    public bool isJumping;
    public bool jumpHeld;
    public bool isFastFalling;
    private Collider bodyCollider;
    private readonly Collider[] groundHits = new Collider[8];


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Make the pawn move, using velocity, in a specified direction
    /// </summary>
    /// <param name="_pawn">A reference to the owning pawn</param>
    /// <param name="_rigidbody">A reference to the owning rigidbody</param>
    /// <param name="_direction">The direction to move in (x-axis is left/right, y-axis is forward/backward, and z-axis is up/down (which is only really used for flying enemies))</param>
    /// <param name="_speed">The speed to move the pawn at (set this to 0 to just use the stats movement speed)</param>
    public void Move(DSPawn _pawn, Vector3 _direction, float _speed=0)
    {
        //if (GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _direction, _speed })) return;
        
        if (_speed == 0)
        {
            _speed = ((DSPawnStats)_pawn.currentStats).movementSpeed;
        }

        var rigidbody = _pawn.GetComponent<Rigidbody>();
        
        // Make sure that the axis passed for the direction are always relative to the direction the pawn is facing
        var localMoveDirection = _pawn.transform.right * _direction.x + _pawn.transform.up * _direction.y + _pawn.transform.forward * _direction.z;
        var currentVelocity = rigidbody.velocity;
        
        bool grounded = IsOnGround(_pawn) && !RecentlyJumped;
        
        // Get desired velocities
        var desiredGroundVelocity = localMoveDirection.normalized * _speed;
        IsOnSlope(_pawn); // Calculate IsOnSlope to get the result of slopeHit
        var slopMoveDirection = Vector3.ProjectOnPlane(localMoveDirection, slopeHit.normal);
        var desiredSlopeVelocity = slopMoveDirection * _speed;
        var desiredAirVelocity = localMoveDirection.normalized * (_speed * ((DSPawnStats)_pawn.currentStats).airMovementMultiplier);
        var desiredCrouchVelocity = localMoveDirection.normalized * (_speed * ((DSPawnStats)_pawn.currentStats).crouchMovementMultiplier);
        
        // Define acceleration rates
        var groundAccelerationRate = ((DSPawnStats)_pawn.currentStats).groundAccelerationRate;
        var slopeAccelerationRate = ((DSPawnStats)_pawn.currentStats).slopeAccelerationRate;
        var airAccelerationRate = ((DSPawnStats)_pawn.currentStats).airAccelerationRate;
        
        // Ground Movement
        if (grounded && !IsOnSlope(_pawn) && !isCrouching)
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((DSPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredGroundVelocity.x && desiredGroundVelocity.x > 0f || currentVelocity.x > desiredGroundVelocity.x && desiredGroundVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredGroundVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredGroundVelocity.y && desiredGroundVelocity.y > 0f || currentVelocity.y > desiredGroundVelocity.y && desiredGroundVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredGroundVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredGroundVelocity.z && desiredGroundVelocity.z > 0f || currentVelocity.z > desiredGroundVelocity.z && desiredGroundVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredGroundVelocity.z*groundAccelerationRate);
            }
        }
        // Crouch Movement
        else if (grounded && !IsOnSlope(_pawn) && isCrouching)
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((DSPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredCrouchVelocity.x && desiredCrouchVelocity.x > 0f || currentVelocity.x > desiredCrouchVelocity.x && desiredCrouchVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredCrouchVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredCrouchVelocity.y && desiredCrouchVelocity.y > 0f || currentVelocity.y > desiredCrouchVelocity.y && desiredCrouchVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredCrouchVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredCrouchVelocity.z && desiredCrouchVelocity.z > 0f || currentVelocity.z > desiredCrouchVelocity.z && desiredCrouchVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredCrouchVelocity.z*groundAccelerationRate);
            }
        }
        // Slope Movement
        else if (grounded && IsOnSlope(_pawn))
        {
            rigidbody.useGravity = false;
            rigidbody.drag = ((DSPawnStats)_pawn.currentStats).slopeDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredSlopeVelocity.x && desiredSlopeVelocity.x > 0f || currentVelocity.x > desiredSlopeVelocity.x && desiredSlopeVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredSlopeVelocity.x*slopeAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredSlopeVelocity.y && desiredSlopeVelocity.y > 0f || currentVelocity.y > desiredSlopeVelocity.y && desiredSlopeVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredSlopeVelocity.y*slopeAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredSlopeVelocity.z && desiredSlopeVelocity.z > 0f || currentVelocity.z > desiredSlopeVelocity.z && desiredSlopeVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredSlopeVelocity.z*slopeAccelerationRate);
            }
        }
        // Air Movement
        else
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((DSPawnStats)_pawn.currentStats).airDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredAirVelocity.x && desiredAirVelocity.x > 0f || currentVelocity.x > desiredAirVelocity.x && desiredAirVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredAirVelocity.x*airAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredAirVelocity.y && desiredAirVelocity.y > 0f || currentVelocity.y > desiredAirVelocity.y && desiredAirVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredAirVelocity.y*airAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredAirVelocity.z && desiredAirVelocity.z > 0f || currentVelocity.z > desiredAirVelocity.z && desiredAirVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredAirVelocity.z*airAccelerationRate);
            }
        }
    }
    
    /// <summary>
    /// TODO Make the pawn move in a direct path to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveTo(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// TODO Make the pawn path-find it's way to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveToSmart(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// Make the pawn turn to face a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void FaceTowardsDirection(DSPawn _pawn, Transform _viewPoint, Vector2 _direction)
    {/*
        //if(GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _viewPoint, _direction })) return;
        
        _viewPoint.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y, 0); // Rotate the body for left/right*/
    }
    
    /// <summary>
    /// Make the pawn face at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void FaceTowardsPosition(DSPawn _pawn, Transform _viewPoint, Vector3 _position, float _speed)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _viewPoint, _position, _speed });
        
        var vectorToTarget = _pawn.transform.position - _position;

        // Rotate the body for left/right
        var bodyLookRotation = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z) * Mathf.Rad2Deg;
        _pawn.transform.rotation = Quaternion.Euler(0, bodyLookRotation+180, 0);
        
        // Rotate the head for up/down
        var headLookRotation = Quaternion.LookRotation(vectorToTarget, _pawn.transform.up).eulerAngles;
        var desiredRotation = new Vector3(-headLookRotation.x, headLookRotation.y + 180, headLookRotation.z);
        _viewPoint.transform.eulerAngles = desiredRotation;
    }
    
    public bool RecentlyJumped => Time.time - lastJumpTime < 0.15f;
    
    public void UpdateCoyoteTime(DSPawn _pawn)
    {
        if (_pawn.GetComponent<Rigidbody>().velocity.y <= 0) isJumping = false;
        if (Time.time - lastJumpTime < 0.2f || isCrouching) return;

        if (IsOnGround(_pawn)) lastGroundedTime = Time.time;
    }

    public bool CanJump(DSPawn _pawn)
    {
        return IsOnGround(_pawn) || Time.time - lastGroundedTime <= _pawn.DSCurrentStats.coyoteTime;
    }

    
    /// <summary>
    /// Make the pawn jump using a force applied to the rigidbody
    /// </summary>
    /// <param name="_pawn">A reference to the pawn to get its jump force & IsOnGround state</param>
    /// <param name="_rigidbody"></param>
    public void Jump(DSPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });

        
        if (CanJump(_pawn) is false)
        {
            Kick(_pawn);
            return;
        }
        
        lastJumpTime = Time.time;
        lastGroundedTime = float.NegativeInfinity;
        
        isJumping = true;
        isFastFalling = false;
        
        var rigidbody = _pawn.GetComponent<Rigidbody>();
        float jumpVelocity = ((DSPawnStats)_pawn.currentStats).jumpForce / rigidbody.mass;
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
    }
    
    /// <summary>
    /// Makes tapping the button do short hops
    /// </summary>
    public void ApplyJumpGravity(DSPawn _pawn)
    {
        // If we are currently in the process of jumping, down try to stub it
        if (isJumping is false || jumpHeld) return;
        
        // If we have already reach the peak of the jump, don't bother with stubbing it since our jump has already completed
        var rigidbody = _pawn.GetComponent<Rigidbody>();
        if (rigidbody.velocity.y <= 0) return;

        // If the player is still on the upwards climb of the jump, apply an extra downwards acceleration
        float extra = ((DSPawnStats)_pawn.currentStats).jumpCutGravity - 1f;
        rigidbody.velocity += Vector3.up * Physics.gravity.y * extra * Time.fixedDeltaTime;
    }

    public void Kick(DSPawn _pawn)
    {
        
    }
    
    /// <summary>
    /// Make the pawn crouch by reducing its capsule collider height (and also trigger Move to change to a crouching movement speed)
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_enable"></param>
    public void Crouch(DSPawn _pawn, bool _enable)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _enable });
        
        if (_enable && isCrouching is false)
        {
            /*var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height -= ((DSPawnStats)_pawn.currentStats).crouchDistance;
            collider.center += ((DSPawnStats)_pawn.currentStats).crouchColliderOffset;*/
            isCrouching = true;
            _pawn.WantsToDrop = true;
        }
        if (_enable is false && isCrouching && IsHeadClear(_pawn))
        {
            /*var collider = _pawn.GetComponent<CapsuleCollider>();
            _pawn.transform.position += new Vector3(0, ((DSPawnStats)_pawn.currentStats).crouchDistance, 0);
            collider.height += ((DSPawnStats)_pawn.currentStats).crouchDistance;
            collider.center -= ((DSPawnStats)_pawn.currentStats).crouchColliderOffset;*/
            isCrouching = false;
            _pawn.WantsToDrop = false;
        }
    }
    
    public void ApplyFastFall(DSPawn _pawn, bool _fallHeld)
    {
        var rigidbody = _pawn.GetComponent<Rigidbody>();

        if (IsOnGround(_pawn) || rigidbody.velocity.y > 0)
        {
            isFastFalling = false;
            return;
        }

        if (_fallHeld && isFastFalling is false)
        {
            isFastFalling = true;
            lastGroundedTime = float.NegativeInfinity;
        }

        if (isFastFalling is false) return;

        var stats = (DSPawnStats)_pawn.currentStats;
        float y = Mathf.MoveTowards(rigidbody.velocity.y, -stats.fastFallSpeed, stats.fastFallAcceleration * Time.fixedDeltaTime);
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, y, rigidbody.velocity.z);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Interact(DSPawn _pawn, GameObject _interactionTrigger, Transform _viewPoint)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _interactionTrigger, _viewPoint });
        
        var interaction = Object.Instantiate(_interactionTrigger, _viewPoint);
        interaction.transform.GetChild(0).GetComponent<VolumeTriggerInteraction>().owningPawn = _pawn;
        Object.Destroy(interaction,  0.2f);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="_action"></param>
    public void ItemUseAction(Pawn_Inventory _inventory, int _action = 0, string _mode = "press")
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _inventory, _action, _mode });
        
        var item = _inventory.GetComponentInChildren<Item>(false);
        if (item is null) return;

        switch (_action)
        {
            case 0:
                item.UsePrimary(_mode);
                break;
            case 1:
                item.UseSecondary(_mode);
                break;
            case 2:
                item.UseTertiary(_mode);
                break;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SwitchItem()
    {
        
    }
    
    public bool IsHeadClear(DSPawn _pawn)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_pawn.transform.position + ((DSPawnStats)_pawn.currentStats).headCheckOffset, ((DSPawnStats)_pawn.currentStats).headCheckRadius, _pawn.transform.up, out hit, ((DSPawnStats)_pawn.currentStats).headCheckDistance, ((DSPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }
    
    public bool IsOnGround(DSPawn _pawn)
    {
        if (bodyCollider == null) bodyCollider = _pawn.GetComponent<Collider>();

        Vector3 crouchingOffset = new Vector3(0, 0, 0);
        if (isCrouching) crouchingOffset = new Vector3(0, ((DSPawnStats)_pawn.currentStats).crouchDistance, 0);

        var stats = (DSPawnStats)_pawn.currentStats;
        int count = Physics.OverlapSphereNonAlloc(
            _pawn.transform.position - stats.groundCheckOffset,
            stats.groundCheckRadius, groundHits, stats.groundMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            var hit = groundHits[i];
            if (hit == bodyCollider) continue;
            if (bodyCollider != null && Physics.GetIgnoreCollision(bodyCollider, hit)) continue;
            return true;
        }
        return false;
    }

    public bool IsOnSlope(DSPawn _pawn)
    {
        /*
        This function does not account for crouching offsets. Meaning if a pawn is crouched, the slope detection will likely fail and the pawn will slip off the slope.
        This is a bug, but I'm deciding to keep it in since it's super fun to be able to crouch when falling at a slope to slide down it!
        If this needs to be patched out for any reason, update this function to account for the crouch offset. If you're not sure how to do that, check IsOnGround function above. It correctly accounts for the crouch offset.
        Happy sliding! ~Liz
        //*/
        if (Physics.Raycast(_pawn.transform.position, Vector3.down, out slopeHit, ((DSPawnStats)_pawn.currentStats).slopeCheckDistance, ((DSPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return slopeHit.normal != Vector3.up;
        }

        return false;
    }

    public void EnableViewCamera(DSPawn _pawn, bool _setActive)
    {
        if (viewCamera is null)
        {
            // Try to get a view camera
            viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;
            if (viewCamera is null) return;
        }
        
        viewCamera.SetActive(_setActive);
    }
    
    public void ItemSwapNext(DSPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchNext();
    }

    public void ItemSwapPrevious(DSPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchPreviouse();
    }
}
