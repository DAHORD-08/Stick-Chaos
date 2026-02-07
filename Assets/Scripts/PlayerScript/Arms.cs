using UnityEngine;

public class Arms : MonoBehaviour
{
    public float speed = 300f;
    public Rigidbody2D rb;
    public Camera cam;
    public int mouseButton; // 0 pour Gauche (Bras Gauche), 1 pour Droit (Bras Droit)

    // Limites de rotation pour éviter les rotations excessives
    private float _targetRotation = 0f;
    private const float RotationLerpSpeed = 0.15f; // Réduire pour plus de douceur

    void FixedUpdate()
    {
        // Conversion position souris en coordonnées monde
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Calcul du vecteur direction entre le bras et la souris
        Vector3 difference = mousePos - transform.position;

        // Calcul de l'angle - CORRIGÉ pour orientation correcte
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90f;

        // Utiliser Lerp pour lisser la rotation et éviter les sauts brusques
        _targetRotation = Mathf.LerpAngle(_targetRotation, rotationZ, RotationLerpSpeed);

        // Si le bouton est maintenu, on force la rotation physique
        if (Input.GetMouseButton(mouseButton))
        {
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, _targetRotation, speed * Time.fixedDeltaTime));
        }
    }
}