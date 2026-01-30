using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 5f)]
    public float parallaxSpeed = 0.5f;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        transform.position += new Vector3(
            deltaMovement.x * parallaxSpeed,
            deltaMovement.y * parallaxSpeed,
            0
        );

        lastCameraPosition = cameraTransform.position;
    }
}
