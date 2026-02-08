using UnityEngine;

public class Arms : MonoBehaviour
{
    public float speed = 300f;
    public Rigidbody2D rb;
    public Camera cam;
    public int mouseButton; // 0 pour Gauche (Bras Gauche), 1 pour Droit (Bras Droit)

    // Référence au script Grab pour vérifier si on est en grab
    private Grab _grabScript;

    private float _targetRotation = 0f;
    private const float RotationLerpSpeed = 0.15f;

    void Start()
    {
        // Récupérer le script Grab s'il existe (sur ce même objet)
        _grabScript = GetComponent<Grab>();
    }

    void FixedUpdate()
    {
        // Si on est en train de grab avec ce bras (main gauche), ne pas tourner
        if (_grabScript != null && _grabScript.IsGrabbing)
        {
            return;
        }

        // Si on maintient le bouton de souris ET qu'on n'est pas en grab, tourner normalement
        if (!Input.GetMouseButton(mouseButton))
        {
            return;
        }

        // Conversion position souris en coordonnées monde
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Calcul du vecteur direction entre le bras et la souris
        Vector3 difference = mousePos - transform.position;

        // Calcul de l'angle
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90f;

        // Utiliser Lerp pour lisser la rotation
        _targetRotation = Mathf.LerpAngle(_targetRotation, rotationZ, RotationLerpSpeed);

        // Appliquer la rotation physique
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, _targetRotation, speed * Time.fixedDeltaTime));
    }
}