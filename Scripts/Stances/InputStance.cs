using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputStance : EmptyStance
{
    [SerializeField] string button = "Button";

    public bool toggle = false;
    protected bool input;

    public override bool OnValidateStance(PlayerController controller)
    {
        base.OnValidateStance(controller);

        if (controller.movement.sqrMagnitude < 0.5f && toggle)
            OnInputEnd();

        bool isValidated = true;

        isValidated &= (controller.onGround || !needsGrounded);
        isValidated &= input;

        return isValidated;
    }

    public override bool CanEnterStance (PlayerController controller)
    {
        bool canEnter = input;

        canEnter &= input;
        canEnter &= (controller.onGround || !needsGrounded);

        return canEnter;
    }

    //Add inputs
    void OnEnable ()
    {
        if (!toggle)
        {
            InputManager.manager.AddEvent(button, OnInputStart, InputType.OnStarted);
            InputManager.manager.AddEvent(button, OnInputEnd, InputType.OnCancelled);
        }
        else
        {
            InputManager.manager.AddEvent(button, OnInputToggle, InputType.OnStarted);
        }
    }

    //Remove inputs
    void OnDisable ()
    {
        if (!toggle)
        {
            InputManager.manager.RemoveEvent(button, OnInputStart, InputType.OnStarted);
            InputManager.manager.RemoveEvent(button, OnInputEnd, InputType.OnCancelled);
        }
        else
        {
            InputManager.manager.RemoveEvent(button, OnInputToggle, InputType.OnStarted);
        }
    }

    ///<Summary>
    ///Called when input is recieved
    ///</Summary>>
    void OnInputStart()
    {
        input = true;
    }

    ///<Summary>
    ///Called when input has stopped being recieved
    ///</Summary>>
    void OnInputEnd()
    {
        input = false;
    }

    ///<Summary>
    ///Called when input is toggled and toggle is enabled
    ///</Summary>>
    void OnInputToggle()
    {
        input = !input;
    }
}
