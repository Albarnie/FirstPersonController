using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSlidingStance", menuName = "Stance/Sliding Stance", order = 3)]
public class SlidingStance : InputStance
{
    public float friction = 0.2f;
    public float control = 0.1f;
    public float minVelocity = 3f;

    Vector3 velocity;

    public override bool CanEnterStance(PlayerController owner)
    {
        bool canEnter = true;

        canEnter &= owner.onGround;
        canEnter &= input;
        canEnter &= owner.lastVelocity.magnitude / Time.fixedDeltaTime > minVelocity;

        return canEnter;
    }

    public override bool OnValidateStance(PlayerController owner)
    {
        bool isValid = true;
        isValid &= owner.onGround;
        isValid &= input;
        isValid &= owner.lastVelocity.magnitude / Time.fixedDeltaTime > minVelocity;

        return isValid;
    }

    public override void OnEnterStance(PlayerController owner)
    {
        base.OnEnterStance(owner);
        velocity = owner.lastVelocity / Time.fixedDeltaTime;
    }

    public override Vector3 OnMove(Vector3 movementInput)
    {
        velocity *= (1 - (friction*Time.deltaTime));
        return velocity + (base.OnMove(movementInput) * control);
    }
}
