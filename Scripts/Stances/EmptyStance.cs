using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EmptyStance : CharacterStance
{
    [SerializeField]
    protected bool needsGrounded = false;

    public override bool OnValidateStance(PlayerController controller)
    {
        bool isValidated = true;

        isValidated &= (controller.onGround ||!needsGrounded);

        return isValidated;
    }

    public override bool CanEnterStance (PlayerController controller)
    {
        bool canEnter = true;

        canEnter &= (controller.onGround || !needsGrounded);

        return canEnter;
    }
}
