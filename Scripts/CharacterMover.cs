using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{
    Rigidbody rb;

    public float normalOffset = 0.005f;
    public float maxStepHeight = 0.1f;

    [Range(0, 1)]
    public float slideAmount = 1f;

    public LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 input, out Vector3 movement)
    {
        Vector3 position = transform.position;
        Vector3 direction = input.normalized;
        float distance = input.magnitude;

        //Perform a sweep test to see if there are any objects in the way
        RaycastHit hit;
        if (rb.SweepTest(direction, out hit, distance))
        {
            float normalDistance = distance - hit.distance;

            position += input * hit.distance;
            position += hit.normal * normalOffset;

            //slide along the surface hit
            Vector3 slide = Vector3.ProjectOnPlane(direction * normalDistance * slideAmount, hit.normal);
            Debug.DrawRay(hit.point, slide / Time.fixedDeltaTime);
            position += slide;
        }
        else
        {
            position += input;
        }
        movement = position - transform.position;
        transform.position = position;
    }

    bool CheckStep (Vector3 direction, ref Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        ray.origin += direction;
        ray.origin += Vector3.up * (maxStepHeight + (normalOffset*2));

        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, maxStepHeight + normalOffset, groundLayer))
        {
            //If the step is below our maximum step height
            if (hit.point.y - position.y < maxStepHeight)
            {
                Debug.DrawLine(position, hit.point, Color.red, 10f);
                position = hit.point;
                return true;
            }
        }
        return false;
    }
}
