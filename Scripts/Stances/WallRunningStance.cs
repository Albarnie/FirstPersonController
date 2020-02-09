using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningStance : InputStance
{
    public float maxWallDistance = 1;
    public float maxWallrunTime;
    public float gravity = 0.1f;
    [Range(0, 1)]
    public float playerControl = 0.5f;

    public Vector3 characterCenter;

    Vector3 wallNormal;
    Vector3 wallPosition;

    Vector3 side;

    public LayerMask wallLayer;

    Rigidbody rb;

    public override bool CanEnterStance(PlayerController controller)
    {
        bool canEnter = true;

        canEnter &= !controller.onGround;
        canEnter &= input;
        canEnter &= CheckSide(Vector3.left) || CheckSide(Vector3.right);

        return canEnter;
    }

    public override bool OnValidateStance(PlayerController controller)
    {
        bool isValidated = true;

        isValidated &= currentStanceTime < maxWallrunTime;
        isValidated &= !controller.onGround;

        isValidated &= CheckSide(side);

        return isValidated;
    }

    bool CheckSide (Vector3 side)
    {
        RaycastHit hit;

        Ray ray = new Ray(transform.position + characterCenter, transform.TransformDirection(side));

        if(Physics.Raycast(ray, out hit, maxWallDistance, wallLayer))
        {
            Debug.DrawLine(transform.position + characterCenter, hit.point, Color.white);
            wallNormal = hit.normal;
            wallPosition = hit.point;
            this.side = side;
            return true;
        }

        return false;
    }

    public override Vector3 OnJump(float jumpForce)
    {
        return jumpForce * Vector3.Lerp(wallNormal, controller.cam.forward, playerControl);
    }

    public override Vector3 OnMove(Vector3 movementInput)
    {
        rb.AddForce((1-gravity)*Physics.gravity*-1, ForceMode.Acceleration);
        Vector3 movement = Vector3.ProjectOnPlane(movementInput, wallNormal);
        movement -= wallNormal;
        return movement;
    }

    public override void OnEnterStance()
    {
        base.OnEnterStance();
        rb = GetComponent<Rigidbody>();
        Vector3 velocity = rb.velocity;
        velocity.y *= 0.2f;
        rb.velocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        
    }
}
