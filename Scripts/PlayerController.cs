using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Albarnie.InputManager;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    CharacterMover mover;

    [Header("Settings")]
    public float movementSpeed = 2;
    public float rotationSpeed = 3;
    public float jumpForce = 4;
    [Range(0, 1)]
    public float movementSmoothness = 0.5f;
    public float rotationSmoothness = 30f;
    public float turnTilt = 10f;

    [SerializeField]
    float footRadius = 0.2f;
    [SerializeField]
    float footOffset = -0.1f;

    public LayerMask groundLayer;

    [Header("References")]
    public Transform cam;
    public Transform character,
        facing;

    [Header("Data")]
    public bool onGround;

    [HideInInspector]
    public Vector3 lastVelocity;
    [HideInInspector]
    public Vector2 movement;
    [HideInInspector]
    public Vector2 smoothMovement;
    [HideInInspector]
    public Vector2 rotation;
    [HideInInspector]
    public float movementDirection;
    [HideInInspector]
    public float currentSpeed;

    Quaternion targetRotation;

    public CharacterStance[] stances;
    public CharacterStance currentStance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mover = GetComponent<CharacterMover>();

        targetRotation = cam.rotation;
        
        DetirmineStance();
    }

    private void OnEnable()
    {
        InputManager.manager.inputs.FindAction("Movement").performed += ctx => movement = (Vector2)ctx.ReadValue<Vector2>();
        InputManager.manager.inputs.FindAction("Movement").canceled += ctx => movement = (Vector2)ctx.ReadValue<Vector2>();

        InputManager.manager.inputs.FindAction("Rotation").performed += ctx => rotation = (Vector2)ctx.ReadValue<Vector2>();
        InputManager.manager.inputs.FindAction("Rotation").canceled += ctx => rotation = (Vector2)ctx.ReadValue<Vector2>();

        InputManager.manager.AddEvent("Jump", Jump);

        for(int i = 0; i < stances.Length; i++)
        {
            CharacterStance original = stances[i];
            stances[i] = Instantiate(stances[i]);
            stances[i].name = original.name;

            stances[i].OnEnableStance(this);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < stances.Length; i++)
        {
            stances[i].OnDisableStance(this);
        }
    }

    private void Update()
    {
        onGround = OnGround();

        DetirmineStance();

        if (currentStance == null)
            return;

        currentStance.OnUpdateStance();

        if (!currentStance.OnValidateStance(this))
        {
            currentStance.OnExitStance();
            currentStance = null;
            DetirmineStance();
        }

        facing.rotation = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0);
        Rotate();
        movementDirection = Vector3.Dot(character.forward, rb.velocity.normalized);
    }

    private void FixedUpdate()
    {
        Move();
    }

    void DetirmineStance ()
    {
        CharacterStance bestStance = currentStance;
        foreach (CharacterStance stance in stances)
        {
            if ((currentStance == null || stance.Priority > bestStance.Priority) && stance.CanEnterStance(this))
            {
                bestStance = stance;
            }
        }

        if(bestStance != currentStance)
        {
            currentStance = bestStance;
            bestStance.OnEnterStance(this);
        }
    }


    void Move()
    {
        smoothMovement = Vector2.Lerp(smoothMovement, movement, Time.fixedDeltaTime * 10);

        Vector3 velocity = new Vector3();
        velocity.x = smoothMovement.x;
        velocity.z = smoothMovement.y;
        velocity = Vector3.Lerp(facing.TransformDirection(velocity), character.TransformDirection(velocity), movementSmoothness);

        float targetSpeed = movementSpeed * currentStance.Speed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 5);

        if (velocity.magnitude > 1)
            velocity.Normalize();

        velocity *= currentSpeed;
        velocity = currentStance.OnMove(velocity);

        mover.Move(velocity * Time.fixedDeltaTime, out lastVelocity);
    }

    void Rotate()
    {
        Quaternion targetRot = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0);
        targetRot *= Quaternion.Euler(-rotation.y * rotationSpeed, 0, 0);
        targetRot *= Quaternion.Euler(0, rotation.x * rotationSpeed, 0);
        targetRot *= Quaternion.Euler(0, 0, rotation.x * rotationSpeed*turnTilt);

        //targetRot = ClampRotationAroundXAxis(targetRot, 80);

        targetRot = ClampDistance(targetRot, character.rotation, new Vector3(-80, -160, -360), new Vector3(80, 160, 360));

        targetRot = currentStance.OnRotate(targetRot);

        targetRotation = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y, targetRot.eulerAngles.z);
        cam.rotation = Quaternion.Lerp(cam.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothness);
    }

    public void AddRotationOffset (Quaternion offset)
    {
        targetRotation *= offset;
    }

    void Jump ()
    {
        if(currentStance.canJump)
        {
            rb.AddForce(currentStance.OnJump(jumpForce), ForceMode.VelocityChange);
        }
    }

    bool OnGround()
    {
        return Physics.OverlapSphere(character.position + (Vector3.up*footOffset), footRadius, groundLayer).Length > 0;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q, float clamp)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -clamp, clamp);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    Quaternion Clamp(Quaternion input, Vector3 min, Vector3 max)
    {
        Vector3 inputEuler = input.eulerAngles;

        //Make sure the angle is between 0 and 360
        inputEuler.x = ClampToRot(inputEuler.x);
        inputEuler.y = ClampToRot(inputEuler.y);
        inputEuler.z = ClampToRot(inputEuler.z);

        inputEuler.x = Mathf.Clamp(inputEuler.x, min.x, max.x);
        inputEuler.y = Mathf.Clamp(inputEuler.y, min.y, max.y);
        inputEuler.z = Mathf.Clamp(inputEuler.z, min.z, max.z);

        return Quaternion.Euler(inputEuler);
    }

    Quaternion ClampDistance(Quaternion input, Quaternion target, Vector3 min, Vector3 max)
    {
        Vector3 inputEuler = input.eulerAngles;
        Vector3 targetEuler = target.eulerAngles;

        inputEuler -= targetEuler;

        //Make sure the angle is between -180 and 180
        inputEuler.x = ClampToRot(inputEuler.x);
        inputEuler.y = ClampToRot(inputEuler.y);
        inputEuler.z = ClampToRot(inputEuler.z);

        Vector3 testEuler = inputEuler;

        inputEuler.x = Mathf.Clamp(inputEuler.x, min.x, max.x);
        inputEuler.y = Mathf.Clamp(inputEuler.y, min.y, max.y);
        inputEuler.z = Mathf.Clamp(inputEuler.z, min.z, max.z);

        inputEuler += targetEuler;

        return Quaternion.Euler(inputEuler);
    }

    float ClampToRot (float angle)
    {
        angle = angle.ClampToCircle();

        if (angle > 180)
        {
            angle = angle - 360;
        }
        return angle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(character.position + (Vector3.up * footOffset), footRadius);

        foreach (CharacterStance stance in stances)
        {
            if (stance != null)
            {
                try
                {
                    stance.OnDrawDebug(transform.position);
                }
                catch
                {

                }
            }
        }
    }
}
