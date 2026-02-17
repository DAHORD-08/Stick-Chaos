using UnityEngine;

/// <summary>
/// Système de saisie (Grab) pour un ragdoll physique.
/// Gère la détection d'objets grabbables, la sélection de la main (ambidextrie dynamique),
/// et la liaison physique via FixedJoint2D ou TargetJoint2D.
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
    [SerializeField] private bool useFixedJoint = false;
    [SerializeField] private float jointFrequency = 20f;
    [SerializeField] private float jointDamping = 0.9f;
    [SerializeField] private bool breakJointOnForce = false;
    [SerializeField] private float breakForce = Mathf.Infinity;

    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGrabDetectionRadius = true;
    [SerializeField] private bool showDetectionDebug = true;

    // État interne
    private GameObject _currentGrabbedObject;
    private Joint2D _currentJoint;
    private Collider2D _currentGrabbableCollider;
    private bool _isGrabbing = false;

    private void Start()
    {
        ValidateComponents();
    }

    private void Update()
    {
        HandleGrabInput();

        // ✅ SUPPRIMER la mise à jour de targetJoint.target
        // Pas besoin puisqu'on utilise FixedJoint2D maintenant

        // Vérifie si l'objet saisi a été détruit
        if (_isGrabbing && _currentGrabbedObject == null)
        {
            ReleaseGrab();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || !showGrabDetectionRadius) return;
        if (leftHand == null || rightHand == null) return;

        // Affiche le rayon de détection du grab
        Gizmos.color = _isGrabbing ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(GetGrabbingHandPosition(), grabDetectionRadius);
    }

    /// <summary>
    /// Valide que tous les composants requis sont assignés.
    /// </summary>
    private void ValidateComponents()
    {
        if (leftHand == null || rightHand == null)
        {
            UnityEngine.Debug.LogError("[RagdollGrabber] ❌ Left Hand et Right Hand doivent être assignés !");
            enabled = false;
            return;
        }

        if (headSprite == null)
        {
            UnityEngine.Debug.LogWarning("[RagdollGrabber] ⚠️ HeadSprite non assigné, la détection d'orientation utilisera localScale.x");
        }

        if (bodyRigidbody == null)
        {
            bodyRigidbody = GetComponent<Rigidbody2D>();
            if (bodyRigidbody == null)
            {
                UnityEngine.Debug.LogError("[RagdollGrabber] ❌ Rigidbody2D du corps non trouvé !");
                enabled = false;
                return;
            }
        }

        // Info sur le LayerMask
        if (showDebugInfo)
        {
            UnityEngine.Debug.Log($"[RagdollGrabber] ✅ LayerMask grabbable configuré : {grabbableLayer.value}");
            UnityEngine.Debug.Log($"[RagdollGrabber] ✅ Rayon de détection : {grabDetectionRadius}m");
        }
    }

    /// <summary>
    /// Gère l'entrée souris pour le grab et le release.
    /// </summary>
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

    /// <summary>
    /// Tente de saisir un objet grabbable à proximité de la main actuelle.
    /// </summary>
    private void AttemptGrab()
    {
        if (_isGrabbing)
        {
            if (showDebugInfo) UnityEngine.Debug.Log("[RagdollGrabber] Déjà en train de saisir quelque chose.");
            return;
        }

        Vector2 handPosition = GetGrabbingHandPosition();
        
        if (showDetectionDebug)
        {
            UnityEngine.Debug.Log($"[RagdollGrabber] 🔍 Tentative de grab à la position : {handPosition}");
            UnityEngine.Debug.Log($"[RagdollGrabber] 🔍 Rayon de détection : {grabDetectionRadius}");
            UnityEngine.Debug.Log($"[RagdollGrabber] 🔍 LayerMask : {grabbableLayer}");
        }

        Collider2D grabbedCollider = DetectGrabbableInRange(handPosition);

        if (grabbedCollider != null)
        {
            CreateGrabJoint(grabbedCollider);
            _isGrabbing = true;

            if (showDebugInfo)
                UnityEngine.Debug.Log($"[RagdollGrabber] ✅ Objet saisi : {grabbedCollider.gameObject.name}");
        }
        else
        {
            if (showDebugInfo)
                UnityEngine.Debug.Log("[RagdollGrabber] ❌ Aucun objet grabbable détecté.");
        }
    }

    /// <summary>
    /// Relâche l'objet actuellement saisi.
    /// </summary>
    private void ReleaseGrab()
    {
        if (!_isGrabbing) return;

        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        _currentGrabbedObject = null;
        _currentGrabbableCollider = null;
        _isGrabbing = false;

        if (showDebugInfo)
            UnityEngine.Debug.Log("[RagdollGrabber] 📤 Objet relâché.");
    }

    /// <summary>
    /// Détecte un objet grabbable à proximité de la main.
    /// </summary>
    private Collider2D DetectGrabbableInRange(Vector2 handPosition)
    {
        // OverlapCircleAll retourne tous les colliders dans le rayon
        Collider2D[] colliders = Physics2D.OverlapCircleAll(handPosition, grabDetectionRadius, grabbableLayer);

        if (showDetectionDebug)
            UnityEngine.Debug.Log($"[RagdollGrabber] 🔍 Colliders trouvés dans le rayon : {colliders.Length}");

        // Cherche le premier collider avec le tag "Grabbable"
        foreach (Collider2D collider in colliders)
        {
            if (collider == null) continue;

            if (showDetectionDebug)
                UnityEngine.Debug.Log($"[RagdollGrabber] 🔍 Collider trouvé : {collider.gameObject.name} (Tag: {collider.gameObject.tag}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)})");

            if (collider.CompareTag(grabbableTag))
            {
                if (showDetectionDebug)
                    UnityEngine.Debug.Log($"[RagdollGrabber] ✅ Collider avec le bon tag trouvé !");
                return collider;
            }
        }

        return null;
    }

    /// <summary>
    /// Crée la liaison physique entre la main et le manche saisi.
    /// </summary>
    private void CreateGrabJoint(Collider2D grabbableCollider)
    {
        _currentGrabbableCollider = grabbableCollider;
        _currentGrabbedObject = grabbableCollider.gameObject;

        // ✅ C'est le Rigidbody du MANCHE qui doit être attaché à la main
        Rigidbody2D handleRigidbody = grabbableCollider.GetComponent<Rigidbody2D>();
        if (handleRigidbody == null)
        {
            UnityEngine.Debug.LogError("[RagdollGrabber] ❌ Le manche n'a pas de Rigidbody2D !");
            return;
        }

        GameObject graspingHand = GetCurrentGrabbingHand();
        
        if (graspingHand == null)
        {
            UnityEngine.Debug.LogError("[RagdollGrabber] ❌ La main n'existe pas !");
            return;
        }

        UnityEngine.Debug.Log($"[RagdollGrabber] 🤚 Main utilisée : {graspingHand.name}");

        Rigidbody2D handRigidbody = graspingHand.GetComponent<Rigidbody2D>();

        if (handRigidbody == null)
        {
            UnityEngine.Debug.LogError($"[RagdollGrabber] ❌ La main '{graspingHand.name}' n'a pas de Rigidbody2D !");
            return;
        }

        UnityEngine.Debug.Log($"[RagdollGrabber] ✅ Hand RB trouvé : {handRigidbody.gameObject.name}");
        UnityEngine.Debug.Log($"[RagdollGrabber] ✅ Handle RB trouvé : {handleRigidbody.gameObject.name}");

        if (useFixedJoint)
        {
            _currentJoint = CreateFixedJoint(handRigidbody, handleRigidbody);
        }
        else
        {
            _currentJoint = CreateTargetJoint(graspingHand, handleRigidbody);
        }

        if (_currentJoint == null)
        {
            UnityEngine.Debug.LogError("[RagdollGrabber] ❌ Le joint n'a pas pu être créé !");
        }
    }

    /// <summary>
    /// Crée un FixedJoint2D pour une liaison rigide.
    /// </summary>
    private FixedJoint2D CreateFixedJoint(Rigidbody2D handRB, Rigidbody2D handleRB)
    {
        FixedJoint2D joint = handRB.gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = handleRB;
        joint.breakForce = breakJointOnForce ? breakForce : Mathf.Infinity;
        joint.breakTorque = breakJointOnForce ? breakForce : Mathf.Infinity;

        if (showDebugInfo)
            UnityEngine.Debug.Log("[RagdollGrabber] ⛓️ FixedJoint2D créé sur la main.");

        return joint;
    }

    /// <summary>
    /// Crée un FixedJoint2D qui relie la main spécifique au manche.
    /// </summary>
    private FixedJoint2D CreateTargetJoint(GameObject hand, Rigidbody2D handleRB)
    {
        // ✅ Obtenir le Rigidbody2D de la main
        Rigidbody2D handRB = hand.GetComponent<Rigidbody2D>();
        if (handRB == null)
        {
            UnityEngine.Debug.LogError($"[RagdollGrabber] ❌ La main {hand.name} n'a pas de Rigidbody2D !");
            return null;
        }

        // ✅ Créer le FixedJoint2D sur la MAIN
        FixedJoint2D joint = handRB.gameObject.AddComponent<FixedJoint2D>();

        // ✅ Connecter la main au manche
        joint.connectedBody = handleRB;
        joint.breakForce = breakJointOnForce ? breakForce : Mathf.Infinity;
        joint.breakTorque = breakJointOnForce ? breakForce : Mathf.Infinity;

        if (showDebugInfo)
            UnityEngine.Debug.Log($"[RagdollGrabber] 🤝 FixedJoint2D créé entre {hand.name} et {handleRB.gameObject.name}");

        return joint;
    }

    /// <summary>
    /// Retourne la position de la main utilisée pour le grab.
    /// </summary>
    private Vector2 GetGrabbingHandPosition()
    {
        GameObject hand = GetCurrentGrabbingHand();
        return hand.transform.position;
    }

    /// <summary>
    /// Retourne la main active selon l'orientation du personnage.
    /// </summary>
    private GameObject GetCurrentGrabbingHand()
    {
        bool facingRight = IsFacingRight();
        return facingRight ? rightHand : leftHand;
    }

    /// <summary>
    /// Détermine l'orientation du personnage.
    /// Utilise d'abord headSprite.flipX si disponible, sinon localScale.x.
    /// </summary>
    private bool IsFacingRight()
    {
        if (headSprite != null)
        {
            return !headSprite.flipX;
        }

        return transform.localScale.x > 0;
    }

    /// <summary>
    /// Retourne l'état actuel du grab.
    /// </summary>
    public bool IsGrabbing => _isGrabbing;

    /// <summary>
    /// Retourne l'objet actuellement saisi.
    /// </summary>
    public GameObject CurrentGrabbedObject => _currentGrabbedObject;

    /// <summary>
    /// Force le relâchement de l'objet.
    /// </summary>
    public void ForceRelease()
    {
        ReleaseGrab();
        if (showDebugInfo)
            UnityEngine.Debug.Log("[RagdollGrabber] 🔓 Relâchement forcé.");
    }
}