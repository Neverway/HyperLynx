//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DSPawnStats : PawnStats
{    
    public override object Clone()
    {
        return MemberwiseClone();
    }
    
    [Header("Movement Speeds")]
    public float movementSpeed = 10;
    public float airMovementMultiplier = 0.25f;
    public float crouchMovementMultiplier = 0.25f;
    [Header("Movement Acceleration")]
    public float groundAccelerationRate = 0.1f;
    public float airAccelerationRate = 0.2f;
    public float slopeAccelerationRate = 0.25f;
    [Header("Movement Drag")]
    public float groundDrag = 8;
    public float airDrag = 0;
    public float slopeDrag = 8;

    [Header("Ground Detection & Jumping")] 
    public float slopeCheckDistance = 0.3f;
    public LayerMask groundMask = 0;
    public float groundCheckRadius = 0.25f;
    public Vector3 groundCheckOffset = new Vector3();
    public float jumpForce = 2600;
    public float coyoteTime = 0.12f;
    public float jumpCutGravity = 3f;
    public float fallForce = 1.5f;
    public float fastFallSpeed = 20f;
    public float fastFallAcceleration = 200f;

    [Header("Head Detection & Crouching")]
    [Tooltip("The radius of the sphere-cast to check if the head is clear, (this value should be smaller than the radius of the body collider to avoid getting false-positives when a pawn is up against a wall)")]
    public float headCheckRadius = 0.25f;
    [Tooltip("The offset from the pawn's origin to start the sphere-cast to check if the head is clear")]
    public Vector3 headCheckOffset = new Vector3(0, 1.75f, 0);
    [Tooltip("The distance of the sphere-cast to check if the head is clear, (This value should normally match, or be greater than, the crouchDistance)")]
    public float headCheckDistance = 0.5f;
    [Tooltip("The amount of height to add/subtract from the collider's height when uncrouching/crouching")]
    public float crouchDistance = 0.5f;
    public Vector3 crouchColliderOffset = new Vector3(0, 0.25f, 0);
}
