using UnityEngine;

/// <summary>
/// Système de saisie (Grab) pour un ragdoll physique.
/// Gère la détection d'objets grabbables, la sélection de la main,
/// le snap physique ultra-précis basé sur les bounds du collider touché.
/// </summary>
public class RagdollGrabber : MonoBehaviour
{
    [Header("=== MAIN COMPONENTS ===")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private SpriteRenderer headSprite;
    [SerializeField] private Rigidbody2D bodyRigidbody;

    [Header("=== GRAB DETECTION SETTINGS ===")]
    [SerializeField] private float grabDetectionRadius = 0.5f;
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private string grabbableTag = "Grabbable";

    [Header("=== JOINT SETTINGS ===")]
    [SerializeField] private bool breakJointOnForce = false;
    [SerializeField] private float breakForce = Mathf.Infinity;

    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGrabDetectionRadius = true;

    // État interne
    private GameObject _currentGrabbedObject;
    private FixedJoint2D _currentJoint;
    private Collider2D[] _ragdollColliders;
    private Collider2D[] _grabbedObjectColliders;
    private bool _isGrabbing = false;

    private void Start()
    {
        ValidateComponents();
        CacheRagdollColliders();
    }

    private void Update()
    {
        HandleGrabInput();

        if (_isGrabbing && _currentGrabbedObject == null)
        {
            ReleaseGrab();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || !showGrabDetectionRadius) return;
        if (leftHand == null || rightHand == null) return;

        Gizmos.color = _isGrabbing ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(GetGrabbingHandPosition(), grabDetectionRadius);
    }

    private void ValidateComponents()
    {
        if (leftHand == null || rightHand == null)
        {
            Debug.LogError("[RagdollGrabber] ❌ Left Hand et Right Hand doivent être assignés !");
            enabled = false;
            return;
        }

        if (headSprite == null)
        {
            Debug.LogWarning("[RagdollGrabber] ⚠️ HeadSprite non assigné, l'orientation utilisera localScale.x");
        }

        if (bodyRigidbody == null)
        {
            bodyRigidbody = GetComponent<Rigidbody2D>();
            if (bodyRigidbody == null)
            {
                Debug.LogError("[RagdollGrabber] ❌ Rigidbody2D du corps non trouvé !");
                enabled = false;
            }
        }
    }

    private void CacheRagdollColliders()
    {
        _ragdollColliders = GetComponentsInChildren<Collider2D>();
    }

    private void HandleGrabInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AttemptGrab();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseGrab();
        }
    }

    private void AttemptGrab()
    {
        if (_isGrabbing) return;

        Vector2 handPosition = GetGrabbingHandPosition();
        Collider2D grabbedCollider = DetectGrabbableInRange(handPosition);

        if (grabbedCollider != null)
        {
            ProcessGrab(grabbedCollider);
        }
        else if (showDebugInfo)
        {
            Debug.Log("[RagdollGrabber] ❌ Aucun objet grabbable détecté.");
        }
    }

    private void ProcessGrab(Collider2D grabbableCollider)
    {
        Rigidbody2D weaponRigidbody = grabbableCollider.attachedRigidbody;

        if (weaponRigidbody == null)
        {
            Debug.LogError("[RagdollGrabber] ❌ L'objet à saisir n'a pas de Rigidbody2D attaché !");
            return;
        }

        GameObject graspingHand = GetCurrentGrabbingHand();
        Rigidbody2D handRigidbody = graspingHand.GetComponent<Rigidbody2D>();

        if (handRigidbody == null) return;

        _currentGrabbedObject = weaponRigidbody.gameObject;

        ManageCollisionsWithGrabbedObject(grabbableCollider, true);
        SnapObjectToHand(grabbableCollider, weaponRigidbody, handRigidbody);

        _currentJoint = CreateOptimizedFixedJoint(grabbableCollider, handRigidbody, weaponRigidbody);
        _isGrabbing = true;

        if (showDebugInfo)
            Debug.Log($"[RagdollGrabber] ✅ Objet saisi et snappé au point de contact : {grabbableCollider.gameObject.name}");
    }

    private void ReleaseGrab()
    {
        if (!_isGrabbing) return;

        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        ManageCollisionsWithGrabbedObject(null, false);

        _currentGrabbedObject = null;
        _isGrabbing = false;

        if (showDebugInfo)
            Debug.Log("[RagdollGrabber] 📤 Objet relâché.");
    }

    private void ManageCollisionsWithGrabbedObject(Collider2D grabbableCollider, bool ignoreCollision)
    {
        if (ignoreCollision && grabbableCollider != null)
        {
            _grabbedObjectColliders = grabbableCollider.transform.root.GetComponentsInChildren<Collider2D>();
        }

        if (_ragdollColliders == null || _grabbedObjectColliders == null) return;

        foreach (Collider2D ragdollCol in _ragdollColliders)
        {
            foreach (Collider2D weaponCol in _grabbedObjectColliders)
            {
                if (ragdollCol != null && weaponCol != null)
                {
                    Physics2D.IgnoreCollision(ragdollCol, weaponCol, ignoreCollision);
                }
            }
        }

        if (!ignoreCollision)
        {
            _grabbedObjectColliders = null;
        }
    }

    /// <summary>
    /// Aligne l'arme en utilisant le centre physique (bounds) du manche, 
    /// forçant Unity à actualiser les positions pour un calcul d'offset sans faille.
    /// </summary>
    private void SnapObjectToHand(Collider2D grabCollider, Rigidbody2D weaponRB, Rigidbody2D handRB)
    {
        // 1. Aligner la rotation globale d'abord
        weaponRB.transform.rotation = handRB.transform.rotation;

        // 2. FORCER Unity à actualiser les Transform enfants après la rotation
        Physics2D.SyncTransforms();

        // 3. Calculer la distance exacte entre la main et le centre de collision du manche
        Vector2 handlePosition = grabCollider.bounds.center;
        Vector2 translationToHand = (Vector2)handRB.transform.position - handlePosition;

        // 4. Déplacer toute l'arme pour annuler cette distance
        weaponRB.transform.position += (Vector3)translationToHand;

        // 5. Seconde synchronisation pour s'assurer que l'ancre du FixedJoint sera correcte
        Physics2D.SyncTransforms();

        // 6. Réinitialiser la dynamique pour la stabilité
        weaponRB.linearVelocity = handRB.linearVelocity;
        weaponRB.angularVelocity = handRB.angularVelocity;
    }

    /// <summary>
    /// Crée un FixedJoint2D dont l'ancre connectée correspond aux coordonnées locales précises du manche.
    /// </summary>
    private FixedJoint2D CreateOptimizedFixedJoint(Collider2D grabCollider, Rigidbody2D handRB, Rigidbody2D weaponRB)
    {
        FixedJoint2D joint = handRB.gameObject.AddComponent<FixedJoint2D>();

        joint.connectedBody = weaponRB;
        joint.autoConfigureConnectedAnchor = false;

        // La main s'accroche depuis son centre géométrique
        joint.anchor = Vector2.zero;

        // L'arme est accrochée au niveau du centre de la boîte de collision de son manche
        joint.connectedAnchor = weaponRB.transform.InverseTransformPoint(grabCollider.bounds.center);

        joint.breakForce = breakJointOnForce ? breakForce : Mathf.Infinity;
        joint.breakTorque = breakJointOnForce ? breakForce : Mathf.Infinity;

        return joint;
    }

    /// <summary>
    /// Cherche l'objet grabbable le PLUS PROCHE dans le rayon défini (sécurité si la lame et le manche ont le même tag).
    /// </summary>
    private Collider2D DetectGrabbableInRange(Vector2 handPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(handPosition, grabDetectionRadius, grabbableLayer);

        Collider2D bestCollider = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            if (col != null && col.CompareTag(grabbableTag))
            {
                float distance = Vector2.Distance(handPosition, col.bounds.center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCollider = col;
                }
            }
        }
        return bestCollider;
    }

    private Vector2 GetGrabbingHandPosition()
    {
        return GetCurrentGrabbingHand().transform.position;
    }

    private GameObject GetCurrentGrabbingHand()
    {
        bool facingRight = (headSprite != null) ? !headSprite.flipX : transform.localScale.x > 0;
        return facingRight ? rightHand : leftHand;
    }

    public bool IsGrabbing => _isGrabbing;
    public GameObject CurrentGrabbedObject => _currentGrabbedObject;

    public void ForceRelease()
    {
        ReleaseGrab();
    }
}