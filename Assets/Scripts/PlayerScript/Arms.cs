using UnityEngine;

public class Arms : MonoBehaviour
{
    public float speed = 300f;
    public Rigidbody2D rb;
    public Camera cam;
    public int mouseButton; // 0 pour Gauche, 1 pour Droit

    private Grab _grabScript;
    private float _targetRotation = 0f;
    private const float RotationLerpSpeed = 0.15f;

    void Start()
    {
        _grabScript = GetComponent<Grab>();
    }

    void FixedUpdate()
    {
        // IMPORTANT : Si on est en grab, NE PAS tourner du tout
        if (_grabScript != null && _grabScript.IsGrabbing)
        {
            return;
        }

        // Si le bouton n'est pas maintenu, ne pas tourner
        if (!Input.GetMouseButton(mouseButton))
        {
            return;
        }

        // Conversion position souris en coordonnées monde
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mousePos - transform.position;

        // Calcul de l'angle
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90f;

        // Lisser la rotation
        _targetRotation = Mathf.LerpAngle(_targetRotation, rotationZ, RotationLerpSpeed);

        // Appliquer la rotation
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, _targetRotation, speed * Time.fixedDeltaTime));
    }
}