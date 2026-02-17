using UnityEngine;

public class WeaponSetup : MonoBehaviour
{
    private void Start()
    {
        // Ignore les collisions entre le manche et la lame
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        if (colliders.Length >= 2)
        {
            Physics2D.IgnoreCollision(colliders[0], colliders[1]);
        }
    }
}