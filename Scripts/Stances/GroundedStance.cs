using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NewGroundedStance", menuName = "Stance/Empty Grounded Stance", order = 3)]
public class GroundedStance : CharacterStance
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
