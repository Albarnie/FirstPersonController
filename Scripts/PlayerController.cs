using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    CharacterMover mover;

    [Header("Settings")]
    public float movementSpeed = 2;
    [SerializeField]
    float rotationSpeed = 3;
    [SerializeField]
    float jumpForce = 4;
    [Range(0, 1), SerializeField]
    float movementSmoothness = 0.5f;

    [SerializeField]
    float footRadius = 0.2f;

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

        stances = GetComponents<CharacterStance>();

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
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        onGround = OnGround();

        DetirmineStance();

        if (currentStance == null)
            return;

        if (!currentStance.OnValidateStance(this))
        {
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
        foreach (CharacterStance stance in stances)
        {
            if ((currentStance == null || stance.Priority > currentStance.Priority) && stance.CanEnterStance(this))
            {
                stance.OnEnterStance();
                currentStance = stance;
            }
        }
    }


    void Move()
    {
        Vector3 velocity = new Vector3();
        velocity.x = movement.x;
        velocity.z = movement.y;
        velocity = Vector3.Lerp(facing.TransformDirection(velocity), character.TransformDirection(velocity), movementSmoothness);

        float targetSpeed = movementSpeed * currentStance.Speed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 5);

        if (velocity.magnitude > 1)
            velocity.Normalize();

        velocity *= currentSpeed;
        velocity = currentStance.OnMove(velocity);

        mover.Move(velocity*Time.fixedDeltaTime, out lastVelocity);
    }

    void Rotate()
    {
        Quaternion targetRot = targetRotation;
        targetRot *= Quaternion.Euler(-rotation.y * rotationSpeed, 0, 0);
        targetRot *= Quaternion.Euler(0, rotation.x * rotationSpeed, 0);

        //targetRot = ClampRotationAroundXAxis(targetRot, 80);

        targetRot = ClampDistance(targetRot, character.rotation, new Vector3(-80, -130, -360), new Vector3(80, 130, 360));

        targetRot = currentStance.OnRotate(targetRot);

        targetRotation = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y, 0);
        cam.rotation = Quaternion.Lerp(cam.rotation, targetRotation, Time.deltaTime*100);
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
        return Physics.OverlapSphere(character.position, footRadius*2, groundLayer).Length > 0;
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
        Gizmos.DrawWireSphere(transform.position, footRadius);
    }
}
