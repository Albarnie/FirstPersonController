using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Albarnie.InputManager;

public enum InteractionType
{
    Hold,
    Toggle,
    Tap
}

[CreateAssetMenu(fileName = "NewInputStance", menuName = "Stance/Input Stance", order = 3)]
public class InputStance : GroundedStance
{
    [SerializeField] string button = "Button";

    public InteractionType interaction;
    protected bool input;
    bool endInputNextFrame = false;

    public override bool OnValidateStance(PlayerController owner)
    {
        base.OnValidateStance(owner);

        if (owner.movement.sqrMagnitude < 0.5f && interaction == InteractionType.Toggle)
            OnInputEnd();

        bool isValidated = true;

        isValidated &= (owner.onGround || !needsGrounded);
        isValidated &= input;

        return isValidated;
    }

    public override bool CanEnterStance (PlayerController owner)
    {
        bool canEnter = input;

        canEnter &= input;
        canEnter &= (owner.onGround || !needsGrounded);

        return canEnter;
    }

    //Add inputs
    public override void OnEnableStance (PlayerController owner)
    {
        this.owner = owner;
        base.OnEnableStance(owner);
        switch (interaction)
        {
            case InteractionType.Hold:
                InputManager.manager.AddEvent(button, OnInputStart, InputType.OnStarted);
                InputManager.manager.AddEvent(button, OnInputEnd, InputType.OnCancelled);
                break;
            case InteractionType.Toggle:
                InputManager.manager.AddEvent(button, OnInputToggle, InputType.OnStarted);
                break;
            case InteractionType.Tap:
                InputManager.manager.AddEvent(button, OnInputTap, InputType.OnStarted);
                break;
        }
    }

    //Remove inputs
    public override void OnDisableStance (PlayerController owner)
    {
        base.OnDisableStance(owner);
        switch (interaction)
        {
            case InteractionType.Hold:
                InputManager.manager.RemoveEvent(button, OnInputStart, InputType.OnStarted);
                InputManager.manager.RemoveEvent(button, OnInputEnd, InputType.OnCancelled);
                break;
            case InteractionType.Toggle:
                InputManager.manager.RemoveEvent(button, OnInputToggle, InputType.OnStarted);
                break;
            case InteractionType.Tap:
                InputManager.manager.RemoveEvent(button, OnInputTap, InputType.OnStarted);
                break;
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

    void OnInputTap ()
    {
        owner.StartCoroutine(InputTap());
    }

    IEnumerator InputTap ()
    {
        input = true;
        yield return null;
        input = false;
    }
}
