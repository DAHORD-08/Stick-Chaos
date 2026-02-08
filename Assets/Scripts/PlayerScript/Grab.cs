using UnityEngine;

public class Grab : MonoBehaviour
{
    private Rigidbody2D _grabbedObjectRB;
    private FixedJoint2D _joint;
    private bool _isGrabbing = false;

    // Propriété publique pour que Arms.cs puisse vérifier l'état du grab
    public bool IsGrabbing => _isGrabbing;

    void Update()
    {
        // Relâcher si on relâche le clic gauche
        if (_isGrabbing && !Input.GetMouseButton(0))
        {
            ReleaseGrab();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifier que c'est un objet grabbable ET qu'on clique
        if (!collision.CompareTag("Grabbable") || !Input.GetMouseButton(0) || _isGrabbing)
        {
            return;
        }

        Rigidbody2D weaponRB = collision.attachedRigidbody;

        if (weaponRB == null)
        {
            UnityEngine.Debug.LogWarning("Le collider Grabbable n'a pas de Rigidbody attaché!");
            return;
        }

        UnityEngine.Debug.Log("✓ Grabbed: " + weaponRB.gameObject.name);

        _grabbedObjectRB = weaponRB;
        _isGrabbing = true;

        // Créer un FixedJoint2D pour lier l'arme au bras
        _joint = gameObject.AddComponent<FixedJoint2D>();
        _joint.connectedBody = _grabbedObjectRB;

        // Paramètres pour un grab fluide
        _joint.dampingRatio = 1f;
        _joint.frequency = 1f;
        _joint.enableCollision = false;

        // Désactiver la gravité de l'arme
        _grabbedObjectRB.gravityScale = 0f;
    }

    private void ReleaseGrab()
    {
        if (!_isGrabbing)
        {
            return;
        }

        UnityEngine.Debug.Log("✓ Released grab");

        _isGrabbing = false;

        if (_joint != null)
        {
            UnityEngine.Debug.Log("Destruction du joint et restauration de l'arme");

            // Détruire le joint
            Destroy(_joint);
            _joint = null;
        }

        if (_grabbedObjectRB != null)
        {
            // Restaurer la gravité
            _grabbedObjectRB.gravityScale = 1f;
            _grabbedObjectRB.linearVelocity = Vector2.zero;
            _grabbedObjectRB.angularVelocity = 0f;
            _grabbedObjectRB = null;
        }
    }
}