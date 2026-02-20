using UnityEngine;

/// <summary>
/// Gère la physique de l'arme avec rotation naturelle.
/// </summary>
public class WeaponPhysics : MonoBehaviour
{
    [Header("=== COMPONENTS ===")]
    [SerializeField] private Rigidbody2D weaponRigidbody;
    [SerializeField] private Collider2D mainCollider;

    [Header("=== PHYSICS SETTINGS ===")]
    [SerializeField] private float maxRotationSpeed = 5f; // Limite la rotation trop rapide
    [SerializeField] private float linearDragOnGround = 0.8f;
    [SerializeField] private float angularDragOnGround = 0.8f;
    [SerializeField] private float linearDragInAir = 0.1f;
    [SerializeField] private float angularDragInAir = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebug = true;

    private bool _isOnGround = false;
    private float _originalAngularDrag;

    private void Start()
    {
        if (weaponRigidbody == null)
            weaponRigidbody = GetComponent<Rigidbody2D>();

        _originalAngularDrag = weaponRigidbody.angularDamping;

        if (showDebug)
            Debug.Log("[WeaponPhysics] ✅ Arme initialisée - Rotation autorisée");
    }

    private void FixedUpdate()
    {
        CheckIfOnGround();
        AdjustDragBasedOnGround();
        LimitRotationSpeed();
    }

    /// <summary>
    /// Vérifie si l'arme est au sol.
    /// </summary>
    private void CheckIfOnGround()
    {
        if (mainCollider == null) return;

        RaycastHit2D hit = Physics2D.Raycast(
            mainCollider.bounds.center,
            Vector2.down,
            0.3f,
            groundLayer
        );

        _isOnGround = hit.collider != null;

        if (showDebug)
            Debug.DrawRay(mainCollider.bounds.center, Vector2.down * 0.3f, _isOnGround ? Color.green : Color.red);
    }

    /// <summary>
    /// Ajuste le drag selon si l'arme est au sol.
    /// </summary>
    private void AdjustDragBasedOnGround()
    {
        if (_isOnGround)
        {
            weaponRigidbody.linearDamping = linearDragOnGround;
            weaponRigidbody.angularDamping = angularDragOnGround;
        }
        else
        {
            weaponRigidbody.linearDamping = linearDragInAir;
            weaponRigidbody.angularDamping = angularDragInAir;
        }
    }

    /// <summary>
    /// Limite la vitesse de rotation pour éviter une rotation folle.
    /// </summary>
    private void LimitRotationSpeed()
    {
        if (weaponRigidbody.angularVelocity > maxRotationSpeed)
        {
            weaponRigidbody.angularVelocity = maxRotationSpeed;
        }
        else if (weaponRigidbody.angularVelocity < -maxRotationSpeed)
        {
            weaponRigidbody.angularVelocity = -maxRotationSpeed;
        }
    }

    public bool IsOnGround => _isOnGround;
}