using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGrabbingStance", menuName = "Stance/Grabbing Stance", order = 3)]
public class GrabbingStance : InputStance
{
    public Vector3 boxOffset;
    public Vector3 boxSize = Vector3.one;

    public LayerMask grabbableMask;

    Rigidbody rb;

    public override bool OnValidateStance(PlayerController controller)
    {
        Vector3 worldBoxOffset = controller.cam.TransformVector(boxOffset);

        bool isValid = true;
        isValid &= !controller.onGround;
        if (isValid)
        {
            Collider[] collisions = Physics.OverlapBox(controller.cam.position + worldBoxOffset, boxSize/2, Quaternion.identity, grabbableMask);
            isValid &= collisions.Length > 0;
        }

        return isValid;
    }

    public override bool CanEnterStance(PlayerController controller)
    {
        Vector3 worldBoxOffset = controller.cam.TransformVector(boxOffset);

        bool isValid = base.CanEnterStance(controller);
        isValid &= !controller.onGround;
        isValid &= input;
        if (isValid)
        {
            Collider[] collisions = Physics.OverlapBox(controller.cam.position + worldBoxOffset, boxSize/2, Quaternion.identity, grabbableMask);
            isValid &= collisions.Length > 0;
        }

        return isValid;
    }

    public override void OnEnterStance(PlayerController owner)
    {
        base.OnEnterStance(owner);
        if(rb == null)
            rb = owner.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public override void OnExitStance ()
    {
        rb.isKinematic = false;
    }

    public override void OnDrawDebug(Vector3 characterPosition)
    {
        Gizmos.matrix *= owner.cam.worldToLocalMatrix;
        Gizmos.DrawWireCube(boxOffset, boxSize);
        Gizmos.matrix *= owner.cam.localToWorldMatrix;
    }
}
