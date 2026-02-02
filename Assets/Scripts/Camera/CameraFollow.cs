using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Glissez ici le "Chest" ou "Body"
    [SerializeField] private float smoothing = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    void FixedUpdate() // Important : FixedUpdate car le Ragdoll est physique
    {
        if (target == null) return;

        // Calcul de la position cible avec l'offset (Z doit rester à -10)
        Vector3 targetPosition = target.position + offset;

        // Lissage du mouvement (Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.fixedDeltaTime);
    }
}