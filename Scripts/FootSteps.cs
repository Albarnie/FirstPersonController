using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    AudioSource source;

    [SerializeField]
    LayerMask groundMask = new LayerMask();

    [SerializeField]
    List<FootstepSurface> surfaces = new List<FootstepSurface>();

    Dictionary<PhysicMaterial, FootstepSurface> materials;

    float delay;

    private void Awake()
    {
        materials = new Dictionary<PhysicMaterial, FootstepSurface>();
        source = GetComponent<AudioSource>();

        foreach(FootstepSurface surface in surfaces)
        {
            foreach(PhysicMaterial mat in surface.materials)
            {
                if(!materials.ContainsKey(mat))
                    materials.Add(mat, surface);
            }
        }
    }

    /// <summary>
    /// Trigger a footstep sound based on the surface the object is walking on.
    /// </summary>
    public void FootStep (AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight < 0.25f || delay > Time.time)
            return;
        FootstepSurface surface;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 5f, groundMask))
        {
            if (hit.collider.sharedMaterial != null && materials.TryGetValue(hit.collider.sharedMaterial, out surface))
            {
                source.clip = surface.sound;
                source.pitch = Random.Range(surface.pitch - 0.1f, surface.pitch + 0.1f) * evt.floatParameter;
                source.Play();
            }
        }
        delay = Time.time + 0.1f;
    }

}

[System.Serializable]
public class FootstepSurface
{
    public PhysicMaterial[] materials;

    public AudioClip sound;

    public float pitch = 1;
}