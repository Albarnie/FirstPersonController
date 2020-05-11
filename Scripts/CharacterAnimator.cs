using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;

    public GameObject ragdollModel, animatedModel;

    Rigidbody rb;
    Animator anim;
    Animator ragdollAnim;

    [Header("Animation")]
    public bool useAnimation = true;
    public bool useRootMotion = true;

    public float bodyWeight = 0.6f, headWeight = 1f;

    [Header("Rotation")]
    float currentRotation;
    Vector3 currentVelocity;

    [Header("IK")]
    public bool useIK = true;
    [Tooltip("  ")]
    public bool useThickFootCast = true;

    [Range(0, 2)]
    public float heightFromGround,
        maxGroundDistance = 0.5f;
    public float bodyYSpeed = 0.3f,
        footIKSpeed = 1;
    public float footRadius = 0.2f;
    public float thickFootOffset = 0.1f;

    public LayerMask IKLayer;

    [Header("Active Ragdoll")]
    public bool useActiveRagdoll;
    public float activeRagdollStrength;

    Vector3 leftFootTargetPosition,
        rightFootTargetPosition;

    float bodyOffset;
    Vector3 leftFootOffset,
        rightFootOffset;

    Quaternion leftFootTargetRotation,
        rightFootTargetRotation;

    AnimatorState currentState;

    Dictionary<Transform, ConfigurableJoint> joints = new Dictionary<Transform, ConfigurableJoint>();
    Dictionary<Transform, Quaternion> startingRotations = new Dictionary<Transform, Quaternion>();

    public Vector3 rootMotion { get; private set; }


    private void Awake()
    {
        rb = controller.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        ragdollAnim = ragdollModel.GetComponent<Animator>();

        for(int i = 0; i < ragdollBones.Length; i++)
        {
            Transform boneTransform = ragdollAnim.GetBoneTransform(ragdollBones[i]);
            joints.Add(boneTransform, boneTransform.GetComponent<ConfigurableJoint>());
            startingRotations.Add(boneTransform, boneTransform.localRotation);
        }

        EndRagdoll();
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case AnimatorState.Ragdoll:
                return;
            case AnimatorState.ActiveRagdoll:
                if (!useActiveRagdoll)
                    break;
                ConstrainRagdoll(activeRagdollStrength);
                return;
            case AnimatorState.Animated:
                return;
            case AnimatorState.None:
                return;
        }
    }
    private void Update()
    {
        switch (currentState)
        {
            case AnimatorState.Ragdoll:
                if (Input.GetKeyDown(KeyCode.C))
                    EndRagdoll();
                break;

            case AnimatorState.ActiveRagdoll:
                if (Input.GetKeyDown(KeyCode.X))
                    EndRagdoll();
                break;

            case AnimatorState.Animated:
                if (Input.GetKeyDown(KeyCode.X))
                {
                    BeginActiveRagdoll();
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    BeginRagdoll();
                }
                break;

            case AnimatorState.None:
                break;
        }

        if (useAnimation)
        {
            Vector3 velocity = Vector3.Lerp(controller.facing.InverseTransformDirection(controller.lastVelocity), controller.character.InverseTransformDirection(controller.lastVelocity), controller.movementSmoothness);

            foreach (CharacterStance stance in controller.stances)
            {
                anim.SetBool(stance.Name, false);
            }
            anim.SetBool(controller.currentStance.Name, true);

            anim.SetBool("Grounded", controller.onGround);

            float rotation = CalcShortestRot(controller.character.eulerAngles.y, controller.cam.eulerAngles.y);
            float roundedRot = Mathf.Round(rotation / 30) * 30;
            currentRotation = Mathf.Lerp(currentRotation, roundedRot > 0 ? Mathf.Ceil(roundedRot / 20) * 20 : Mathf.Floor(roundedRot / 20) * 20, 2 * Time.deltaTime);
            anim.SetFloat("YRot", currentRotation * 1.5f);

            currentVelocity = Vector3.Lerp(currentVelocity, velocity, Time.deltaTime * 5);

            anim.SetFloat("ZSpeed", currentVelocity.z / Time.fixedDeltaTime / controller.movementSpeed);
            if (!controller.onGround)
                anim.SetFloat("YSpeed", rb.velocity.y);
            anim.SetFloat("XSpeed", currentVelocity.x / Time.fixedDeltaTime / controller.movementSpeed);
        }
    }

    private void OnAnimatorMove()
    {
        if (useRootMotion)
        {
            rootMotion = anim.deltaPosition;
            if (controller.onGround)
            {
                transform.Rotate(anim.deltaRotation.eulerAngles);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, controller.cam.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime * 10);
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null)
            return;

        anim.SetLookAtPosition(controller.cam.position + controller.cam.forward);
        anim.SetLookAtWeight(1, bodyWeight, headWeight, 1);

        if (useIK && controller.onGround)
        {
            //Find the IK positions
            FindIKPosition(HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot, ref leftFootOffset, ref leftFootTargetPosition, ref leftFootTargetRotation);
            FindIKPosition(HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot, ref rightFootOffset, ref rightFootTargetPosition, ref rightFootTargetRotation);

            //Set the IK positions
            SetFootIK(leftFootTargetPosition, leftFootTargetRotation, AvatarIKGoal.LeftFoot);
            SetFootIK(rightFootTargetPosition, rightFootTargetRotation, AvatarIKGoal.RightFoot);

            //Move the body to the lowest foot
            MoveBody();

            //Set weights 
            //Rotation
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            //Position
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        }
        else if(useIK)
        {
            bodyOffset = 0;
        }
        else
        {

            //Set weights 
            //Rotation
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            //Position
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    /// <summary>
    /// Moves the position of the body so that the lowest foot is fully extended
    /// </summary>
    void MoveBody(bool inAir = false)
    {
        //Choose the lowest foot
        float offset = leftFootOffset.y < rightFootOffset.y ? leftFootOffset.y : rightFootOffset.y;

        if (inAir)
        {
            offset = 0;
        }

        //Interpolate the body offset
        bodyOffset = Mathf.Lerp(bodyOffset, offset, bodyYSpeed * Time.deltaTime);

        //Set the new body position
        Vector3 newBodyPosition = anim.bodyPosition;
        newBodyPosition.y += bodyOffset;
        anim.bodyPosition = newBodyPosition;
    }

    /// <summary>
    /// Sets the foot IK position and rotation
    /// </summary>
    void SetFootIK(Vector3 footTargetPosition, Quaternion footTargetRotation, AvatarIKGoal foot)
    {
        anim.SetIKPosition(foot, footTargetPosition);
        anim.SetIKRotation(foot, footTargetRotation);
    }

    /// <summary>
    /// Find the position of the the surface and offset the foot IK to it
    /// </summary>
    bool FindIKPosition(HumanBodyBones foot, AvatarIKGoal footIK, ref Vector3 footOffset, ref Vector3 footTargetPosition, ref Quaternion footTargetRotation)
    {
        bool footOnGround = false;
        RaycastHit hit;

        //Get the original foot position and rotation
        Vector3 footPosition = anim.GetBoneTransform(foot).position;
        Quaternion footRotation = anim.GetIKRotation(footIK);

        Vector3 newOffset = Vector3.zero;

        Ray ray = new Ray(footPosition + (Vector3.up * heightFromGround), Vector3.down);
        //If the foot is close to a surface
        if (Physics.Raycast(ray, out hit, maxGroundDistance + heightFromGround, IKLayer))
        {
            newOffset.y = hit.point.y - transform.position.y;

            footOnGround = true;
        }
        else if (useThickFootCast && Physics.SphereCast(ray, footRadius, out hit, maxGroundDistance + heightFromGround - (footRadius * 2), IKLayer))
        {
            newOffset = (hit.point + (Vector3.up*thickFootOffset)) - footPosition;
            footOnGround = true;
        }


        //Interpolate the foot offset
        footOffset = Vector3.Lerp(footOffset, newOffset, footIKSpeed * Time.deltaTime);

        //Set the foot IK position
        footTargetPosition = footPosition;
        footTargetPosition += footOffset;

        //Get the angle and axis of the ground, and rotate our foot rotation by that offset

        float angle = Vector3.Angle(transform.up, hit.normal);
        Vector3 axis = Vector3.Cross(transform.up, hit.normal);

        Quaternion angleOffset = Quaternion.AngleAxis(angle, axis);

        Debug.DrawRay(footPosition, hit.normal, Color.blue);
        Debug.DrawRay(footPosition, transform.forward);
        Debug.DrawRay(footPosition, axis, Color.red);

        footTargetRotation = footRotation * angleOffset;

        return footOnGround;
    }

    void BeginRagdoll()
    {
        MatchRagdoll(1);

        ragdollModel.SetActive(true);
        animatedModel.SetActive(false);
        currentState = AnimatorState.Ragdoll;

        anim.SetFloat("HurtAmount", 1);
        anim.SetBool("Ragdoll", true);

        foreach(ConfigurableJoint joint in joints.Values)
        {
            JointDrive xDrive = joint.xDrive;
            xDrive.maximumForce = 0;
            JointDrive yDrive = joint.yDrive;
            yDrive.maximumForce = 0;
            JointDrive zDrive = joint.zDrive;
            zDrive.maximumForce = 0;

            joint.xDrive = xDrive;
            joint.yDrive = yDrive;
            joint.zDrive = zDrive;

            JointDrive angularXDrive = joint.angularXDrive;
            angularXDrive.maximumForce = 0;
            JointDrive angularYZDrive = joint.angularYZDrive;
            angularYZDrive.maximumForce = 0;

            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
        }
    }

    void BeginActiveRagdoll()
    {
        MatchRagdoll(1);

        ragdollModel.SetActive(true);
        animatedModel.SetActive(false);
        currentState = AnimatorState.ActiveRagdoll;

        anim.SetFloat("HurtAmount", 0);
        anim.SetBool("Ragdoll", true);

        foreach (ConfigurableJoint joint in joints.Values)
        {
            JointDrive xDrive = joint.xDrive;
            xDrive.maximumForce = activeRagdollStrength;
            JointDrive yDrive = joint.yDrive;
            yDrive.maximumForce = activeRagdollStrength;
            JointDrive zDrive = joint.zDrive;
            zDrive.maximumForce = activeRagdollStrength;

            joint.xDrive = xDrive;
            joint.yDrive = yDrive;
            joint.zDrive = zDrive;

            JointDrive angularXDrive = joint.angularXDrive;
            angularXDrive.maximumForce = activeRagdollStrength;
            JointDrive angularYZDrive = joint.angularYZDrive;
            angularYZDrive.maximumForce = activeRagdollStrength;

            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
        }
    }

    void EndRagdoll()
    {
        ragdollModel.SetActive(false);
        animatedModel.SetActive(true);
        currentState = AnimatorState.Animated;

        anim.SetBool("Ragdoll", false);
    }

    void ConstrainRagdoll(float weight)
    {
        foreach (HumanBodyBones bone in ragdollBones)
        {
            ConstrainBone(bone, weight);
        }
    }

    void MatchRagdoll (float weight)
    {
        ragdollAnim.bodyPosition = Vector3.Lerp(ragdollAnim.bodyPosition, anim.bodyPosition, weight);
        for(int i = 0; i < System.Enum.GetNames(typeof(HumanBodyBones)).Length-1; i++)
        {
            MatchBone((HumanBodyBones)i, weight);
        }
    }

    void ConstrainBone(HumanBodyBones bone, float weight)
    {
        //Get the bones
        Transform ragdollBone = ragdollAnim.GetBoneTransform(bone);
        Transform animatedBone = anim.GetBoneTransform(bone);

        //Get the bone joint
        ConfigurableJoint joint;
        joints.TryGetValue(ragdollBone, out joint);

        //Get the bone's starting rotation
        Quaternion startingRotation;
        startingRotations.TryGetValue(ragdollBone, out startingRotation);

        joint.SetTargetRotationLocal(animatedBone.localRotation, startingRotation);
    }

    void MatchBone (HumanBodyBones bone, float weight)
    {
        Transform ragdollBone = ragdollAnim.GetBoneTransform(bone);
        if (ragdollBone == null)
            return;
        Transform animatedBone = anim.GetBoneTransform(bone);

        Debug.DrawLine(ragdollBone.position, animatedBone.position);

        ragdollBone.rotation = Quaternion.Lerp(ragdollBone.rotation, animatedBone.rotation, weight);
        ragdollBone.position = Vector3.Lerp(ragdollBone.position, animatedBone.position, weight);
    }

    float CalcShortestRot(float from, float to)
    {
        // If from or to is a negative, we have to recalculate them.
        // For an example, if from = -45 then from(-45) + 360 = 315.
        if (from < 0)
        {
            from += 360;
        }
        if (to < 0)
        {
            to += 360;
        }

        // Do not rotate if from == to.
        if (from == to ||
           from == 0 && to == 360 ||
           from == 360 && to == 0)
        {
            return 0;
        }

        // Pre-calculate left and right.
        float left = (360 - from) + to;
        float right = from - to;
        // If from < to, re-calculate left and right.
        if (from < to)
        {
            if (to > 0)
            {
                left = to - from;
                right = (360 - to) + from;
            }
            else
            {
                left = (360 - to) + from;
                right = to - from;
            }
        }

        // Determine the shortest direction.
        return ((left <= right) ? left : (right * -1));
    }

    public enum AnimatorState
    {
        Ragdoll,
        ActiveRagdoll,
        Animated,
        None
    };

    public readonly HumanBodyBones[] ragdollBones =
    {
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,

            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,

            HumanBodyBones.Spine,
            HumanBodyBones.Chest,

            HumanBodyBones.Neck,
            HumanBodyBones.Head
        //HumanBodyBones.UpperChest,

        //HumanBodyBones.Hips
    };
}
