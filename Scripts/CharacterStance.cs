using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStance : MonoBehaviour
{
    [SerializeField]
    int priority = 0;

    [SerializeField]
    float speed = 1f;

    [SerializeField]
    new string name = "Stance";

    public bool canJump;

    protected float currentStanceTime;

    protected PlayerController controller;

    #region Properties
    public int Priority
    {
        get
        {
            return priority;
        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    public string Name
    {
        get
        {
            return name;
        }
    }

    #endregion

    /// <summary>
    /// Called to detirmine whether to stay in the state or to look for a new one
    /// </summary>
    public virtual bool OnValidateStance(PlayerController controller)
    {
        currentStanceTime += Time.deltaTime;
        return true;
    }

    /// <summary>
    /// Called to detirmine whether the stance can be entered
    /// </summary>
    public abstract bool CanEnterStance(PlayerController controller);

    /// <summary>
    /// Move the character
    /// </summary>
    public virtual Vector3 OnMove(Vector3 movementInput)
    {
        return movementInput;
    }

    /// <summary>
    /// Rotate the character
    /// </summary>
    public virtual Quaternion OnRotate(Quaternion rotationInput)
    {
        return rotationInput;
    }

    public virtual Vector3 OnJump (float jumpForce)
    {
        return Vector3.up*jumpForce;
    }

    public virtual void OnEnterStance ()
    {
        controller = GetComponent<PlayerController>();
        currentStanceTime = 0;
    }
}
