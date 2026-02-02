using UnityEngine;

public class ActiveArm : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    private Camera _mainCam;

    [Header("Physics Settings")]
    public float torqueForce = 1500f; // Force de rotation
    public float damping = 40f;      // Amortissement pour éviter les tremblements
    public float rotationOffset = 0f; // Ajustez si le bras pointe vers le haut/bas au lieu de la souris

    [HideInInspector] public bool isActive = false;

    void Start()
    {
        _mainCam = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;

        // Calcul du torque avec amortissement (Damping)
        float angleError = Mathf.DeltaAngle(rb.rotation, targetAngle);
        float torque = (angleError * torqueForce) - (rb.angularVelocity * damping);

        rb.AddTorque(torque);
    }

    private void RotateTowardsMouse()
    {
        // 1. Position de la souris en monde
        Vector3 mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Calcul du vecteur directionnel
        Vector2 direction = mousePos - transform.position;

        // 3. Calcul de l'angle cible
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;

        // 4. Calcul de l'erreur d'angle et application du torque (Couple)
        float angleError = Mathf.DeltaAngle(rb.rotation, targetAngle);
        float torque = (angleError * torqueForce) - (rb.angularVelocity * damping);

        rb.AddTorque(torque);
    }
}