using UnityEngine;

public class Grab : MonoBehaviour
{
    private bool _hold = false;
    public int mouseButton; // Doit correspondre au bouton du bras associé
    private FixedJoint2D _joint;

    void Update()
    {
        // On vérifie si on maintient le clic pour savoir si on veut "tenir"
        if (Input.GetMouseButton(mouseButton))
        {
            _hold = true;
        }
        else
        {
            _hold = false;
            // Si on relâche, on détruit le lien physique
            if (_joint != null)
            {
                Destroy(_joint);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si on touche un objet avec un Rigidbody en maintenant le clic
        if (_hold && collision.rigidbody != null && _joint == null)
        {
            // On crée dynamiquement un FixedJoint2D pour "coller" l'objet à la main
            _joint = gameObject.AddComponent<FixedJoint2D>();
            _joint.connectedBody = collision.rigidbody;
        }
    }
}