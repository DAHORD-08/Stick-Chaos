using UnityEngine;

public class RagdollGrabber : MonoBehaviour
{
    public LayerMask ragdollLayer;
    public float grabRadius = 1f;
    public Transform grabPoint;
    private GameObject grabbedRagdoll;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Replace "Fire1" with your input scheme
        {
            TryGrabRagdoll();
        }
        if (Input.GetButtonUp("Fire1") && grabbedRagdoll != null)
        {
            ReleaseRagdoll();
        }
    }

    void TryGrabRagdoll()
    {
        Collider[] ragdolls = Physics.OverlapSphere(grabPoint.position, grabRadius, ragdollLayer);
        if (ragdolls.Length > 0)
        {
            grabbedRagdoll = ragdolls[0].gameObject;
            // Set the ragdoll to the grab point position
            grabbedRagdoll.transform.position = grabPoint.position;
        }
    }

    void ReleaseRagdoll()
    {
        grabbedRagdoll = null;
    }
}