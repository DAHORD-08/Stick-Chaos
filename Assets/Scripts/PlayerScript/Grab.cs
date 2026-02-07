using UnityEngine;

public class Grab : MonoBehaviour
{
    private Rigidbody2D _grabbedObjectRB;
    private Rigidbody2D _armRB;
    private Vector3 _grabOffset; // Offset entre la main et l'arme
    private bool _isGrabbing = false;

    void Start()
    {
        _armRB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Relâcher le grip quand on relâche le bouton gauche
        if (!Input.GetMouseButton(0))
        {
            ReleaseGrab();
        }
    }

    void FixedUpdate()
    {
        // Si on grab, attirer l'arme DOUCEMENT vers la main
        if (_isGrabbing && _grabbedObjectRB != null)
        {
            Vector3 directionToHand = (transform.position - _grabbedObjectRB.transform.position);

            // Force très faible pour un mouvement fluide
            float grabForce = 5f; // À ajuster si nécessaire
            _grabbedObjectRB.linearVelocity = directionToHand.normalized * grabForce;

            // Arrêter la rotation de l'arme
            _grabbedObjectRB.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Entrer en contact avec un objet grabbable ET maintenir le clic
        if (collision.CompareTag("Grabbable") && Input.GetMouseButton(0) && !_isGrabbing)
        {
            Rigidbody2D weaponRB = collision.attachedRigidbody;

            if (weaponRB != null)
            {

                _grabbedObjectRB = weaponRB;
                _isGrabbing = true;

                // Désactiver la gravité de l'arme
                _grabbedObjectRB.gravityScale = 0f;

                // Calculer l'offset
                _grabOffset = _grabbedObjectRB.transform.position - transform.position;
            }
        }
    }

    private void ReleaseGrab()
    {
        if (_isGrabbing)
        {
            _isGrabbing = false;

            if (_grabbedObjectRB != null)
            {
                _grabbedObjectRB.gravityScale = 1f;
                _grabbedObjectRB.linearVelocity = Vector2.zero;
                _grabbedObjectRB = null;
            }
        }
    }
}