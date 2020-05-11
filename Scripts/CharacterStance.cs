using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewStance", menuName ="Stance/Empty Stance", order = 3)]
public abstract class CharacterStance : ScriptableObject
{
    [Header("Stance Settings")]

    [SerializeField]
    int priority = 0;

    [SerializeField]
    float speed = 1f;

    public bool canJump;

    protected float currentStanceTime;

    protected PlayerController owner;

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
    /// Called every frame to detirmine whether to stay in the state or to look for a new one
    /// </summary>
    public virtual bool OnValidateStance(PlayerController controller)
    {
        return true;
    }

    /// <summary>
    /// Called to detirmine whether the stance can be entered
    /// </summary>
    public abstract bool CanEnterStance(PlayerController controller);

    /// <summary>
    /// Called every Physics Update to detirmine the movement of the character
    /// </summary>
    /// <param name="movementInput"></param>
    /// <returns></returns>
    public virtual Vector3 OnMove(Vector3 movementInput)
    {
        return movementInput;
    }

    /// <summary>
    /// Called every frame to detirmine the rotation of the player.
    /// </summary>
    /// <param name="rotationInput">The current rotation of the player (body and camera) in local space</param>
    /// <returns>The final rotation of the character</returns>
    public virtual Quaternion OnRotate(Quaternion rotationInput)
    {
        return rotationInput;
    }

    /// <summary>
    /// Called when the player jumps
    /// </summary>
    /// <param name="jumpForce">The input force of the jump</param>
    /// <returns>The final force of the jump</returns>
    public virtual Vector3 OnJump (float jumpForce)
    {
        return Vector3.up*jumpForce;
    }

    /// <summary>
    /// Called when the stance becomes active on the player
    /// </summary>
    /// <param name="owner">The owner player controller</param>
    public virtual void OnEnterStance (PlayerController owner)
    {
        this.owner = owner;
        currentStanceTime = 0;
    }

    /// <summary>
    /// Called when the stance stops being active on the player
    /// </summary>
    public virtual void OnExitStance()
    {
        currentStanceTime = 0;
    }

    /// <summary>
    /// Called every frame while the stance is active
    /// </summary>
    public virtual void OnUpdateStance ()
    {
        currentStanceTime += Time.deltaTime;
    }

    public virtual void OnDrawDebug (Vector3 characterPosition)
    {

    }

    public virtual void OnEnableStance(PlayerController owner)
    {

    }

    public virtual void OnDisableStance(PlayerController owner)
    {

    }
}
