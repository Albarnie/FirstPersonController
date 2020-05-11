using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWallrunningStance", menuName = "Stance/Wallrunning Stance", order = 3)]
public class WallRunningStance : InputStance
{
    [Header("Requirements")]
    public float maxWallDistance = 1;
    public float maxWallrunTime = 4f;
    public float maxVerticalVelocity = 2;
    public float maxAngle = 120;

    [Header("Wallrun Settings")]
    public float gravity = 0.3f;

    [Range(0, 1)]
    public float jumpControl = 0.3f;

    public float jumpMultiplier = 1.5f;

    public float entranceSpeed = 4f;
    public float cameraAngle = 20f;

    public Vector3 characterCenter;
    public LayerMask wallLayer;
    Vector3 wallNormal;
    Vector3 wallPosition;

    Vector3 side;

    Rigidbody rb;

    public override bool CanEnterStance(PlayerController owner)
    {
        this.owner = owner;
        if (rb == null)
            rb = owner.GetComponent<Rigidbody>();

        bool canEnter = true;
        canEnter &= !owner.onGround;
        canEnter &= input;
        canEnter &= Mathf.Abs(rb.velocity.y) < maxVerticalVelocity;

        canEnter &= CheckSide(Vector3.left, owner.facing) || CheckSide(Vector3.right, owner.facing);
        float angle = Vector3.Angle(rb.velocity.normalized, -wallNormal);
        canEnter &= angle < maxAngle;

        return canEnter;
    }

    public override bool OnValidateStance(PlayerController owner)
    {
        bool isValidated = true;

        isValidated &= currentStanceTime < maxWallrunTime;
        isValidated &= !owner.onGround;
        isValidated &= Mathf.Abs(rb.velocity.y) < maxVerticalVelocity;

        isValidated &= Vector3.Angle(owner.facing.TransformDirection(side), -wallNormal) < maxAngle;
        float angle = Vector3.Angle(rb.velocity.normalized, -wallNormal);
        isValidated &= angle < maxAngle;

        CheckSide(owner.facing.InverseTransformDirection(-wallNormal), owner.facing, false);

        return isValidated;
    }

    bool CheckSide(Vector3 side, Transform origin, bool setFacing = true)
    {
        RaycastHit hit;
        Ray ray = new Ray(owner.transform.position + characterCenter, origin.TransformDirection(side));
        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        //Check if there is a wall on this size
        if (Physics.Raycast(ray, out hit, maxWallDistance, wallLayer))
        {
            Debug.DrawLine(owner.transform.position + characterCenter, hit.point, Color.white);
            wallNormal = hit.normal;
            wallPosition = hit.point;
            if (setFacing)
            {
                this.side = side;
            }

            return true;
        }

        return false;
    }

    public override Vector3 OnJump(float jumpForce)
    {
        return jumpForce * jumpMultiplier * Vector3.Lerp(wallNormal, owner.cam.forward, jumpControl);
    }

    public override Vector3 OnMove(Vector3 movementInput)
    {
        rb.AddForce((1 - gravity) * Physics.gravity * -1, ForceMode.Acceleration);
        Vector3 movement = Vector3.ProjectOnPlane(movementInput, wallNormal);
        movement -= wallNormal * 2;
        return movement;
    }

    public override void OnEnterStance(PlayerController owner)
    {
        base.OnEnterStance(owner);

        Vector3 velocity = rb.velocity;
        velocity.y *= gravity;
        rb.velocity = velocity;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, entranceSpeed);
    }

    public override Quaternion OnRotate(Quaternion rotationInput)
    {
        Vector3 localWallPos = owner.cam.InverseTransformPoint(wallPosition);
        Vector3 localPos = owner.cam.InverseTransformPoint(owner.transform.position);

        float currentCameraAngle = cameraAngle;
        currentCameraAngle *= (Vector3.Dot(owner.facing.TransformVector(side), wallNormal));

        //Rotate away from the wall
        return rotationInput * Quaternion.Euler(0, 0, side.x > 0 ? -currentCameraAngle : currentCameraAngle);
    }
}
