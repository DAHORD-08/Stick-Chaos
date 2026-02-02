using UnityEngine;

public class Grabber : MonoBehaviour
{
    private FixedJoint2D _joint;
    private GameObject _potentialTarget;
    private bool _isGrabbing = false;

    // On définit quelle touche active cette main (ex: Mouse0 pour gauche, Mouse1 pour droite)
    [SerializeField] private MouseButton mouseButton;
    private enum MouseButton { Left = 0, Right = 1 }

    void Update()
    {
        if (Input.GetMouseButtonDown((int)mouseButton))
        {
            TryGrab();
        }

        if (Input.GetMouseButtonUp((int)mouseButton))
        {
            Release();
        }
    }

    private void TryGrab()
    {
        if (_potentialTarget != null && !_isGrabbing)
        {
            _isGrabbing = true;

            // On crée le joint dynamiquement
            _joint = gameObject.AddComponent<FixedJoint2D>();
            _joint.connectedBody = _potentialTarget.GetComponent<Rigidbody2D>();

            // Optionnel : limite la force pour que le joint casse si l'objet est trop lourd
            _joint.breakForce = 5000;
        }
    }

    private void Release()
    {
        if (_isGrabbing)
        {
            _isGrabbing = false;
            Destroy(_joint);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Grabbable"))
        {
            _potentialTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Grabbable") && !_isGrabbing)
        {
            _potentialTarget = null;
        }
    }
}