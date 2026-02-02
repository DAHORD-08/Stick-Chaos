using UnityEngine;

public class Arms : MonoBehaviour
{
    public float speed = 300f;
    public Rigidbody2D rb;
    public Camera cam;
    public int mouseButton; // 0 pour Gauche (Bras Gauche), 1 pour Droit (Bras Droit)

    void FixedUpdate()
    {
        // Conversion position souris en coordonnées monde
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Calcul du vecteur direction entre le bras et la souris
        Vector3 difference = mousePos - transform.position;

        // Calcul de l'angle (Note : l'inversion des axes dans Atan2 est spécifique au tuto pour l'orientation)
        float rotationZ = Mathf.Atan2(difference.x, -difference.y) * Mathf.Rad2Deg;

        // Si le bouton est maintenu, on force la rotation physique
        if (Input.GetMouseButton(mouseButton))
        {
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, rotationZ, speed * Time.fixedDeltaTime));
        }
    }
}