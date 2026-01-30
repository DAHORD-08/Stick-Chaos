using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject leftLeg;
    [SerializeField] private GameObject rightLeg;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform playerPos;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float stepWait = 0.5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Collision Settings")]
    [SerializeField] private float positionRadius = 0.2f;
    [SerializeField] private LayerMask ground;

    [Header("Graphics Reference")]
    [SerializeField] private Transform GFX_Container;

    private Rigidbody2D _leftLegRB;
    private Rigidbody2D _rightLegRB;
    private bool _isOnGround;
    private bool _isWalking;

    private void Start()
    {
        if (leftLeg != null) _leftLegRB = leftLeg.GetComponent<Rigidbody2D>();
        if (rightLeg != null) _rightLegRB = rightLeg.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleJumpInput();
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {

            FlipCharacter(horizontalInput);

            if (!_isWalking)
            {
                anim.Play("Walk");
                StartCoroutine(WalkCycle(horizontalInput));
            }
        }
        else
        {
            _isWalking = false;
            StopAllCoroutines();
            anim.Play("Idle");
        }
    }

    private void HandleJumpInput()
    {
        _isOnGround = Physics2D.OverlapCircle(playerPos.position, positionRadius, ground);

        // GetButtonDown("Jump") gère Espace par défaut dans Unity.
        // Pour ZQSD/WASD/Flèches, on vérifie si l'axe Vertical vient de dépasser un seuil.
        bool jumpKeyPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.W);

        if (_isOnGround && jumpKeyPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
        }
    }

    private IEnumerator WalkCycle(float direction)
    {
        _isWalking = true;
        Vector2 force = new Vector2(direction, 0) * (speed * 1000);

        while (_isWalking)
        {
            _leftLegRB.AddForce(force * Time.deltaTime);
            yield return new WaitForSeconds(stepWait);

            _rightLegRB.AddForce(force * Time.deltaTime);
            yield return new WaitForSeconds(stepWait);
        }
    }

    private void FlipCharacter(float direction)
    {
        if (GFX_Container == null) return;

        float currentX = GFX_Container.localScale.x;
        float desiredSign = direction > 0 ? 1f : -1f;

        // Si le signe est déjà celui désiré, rien à faire
        if (Mathf.Sign(currentX) == desiredSign) return;

        // --- Collecte et sauvegarde des rigidbodies enfants (positions, rotations, vitesses) ---
        Rigidbody2D[] childRbs = GFX_Container.GetComponentsInChildren<Rigidbody2D>(true);

        Vector2[] savedPositions = new Vector2[childRbs.Length];
        float[] savedRotations = new float[childRbs.Length];
        Vector2[] savedVelocities = new Vector2[childRbs.Length];

        for (int i = 0; i < childRbs.Length; i++)
        {
            savedPositions[i] = childRbs[i].position;
            savedRotations[i] = childRbs[i].rotation;
            savedVelocities[i] = childRbs[i].linearVelocity;

            // Désactive temporairement la simulation pour éviter recomputations physiques durant le flip
            childRbs[i].simulated = false;
        }

        // --- Sauvegarde de la position du joueur (physique principal) comme auparavant ---
        Vector3 savedWorldPos;
        if (rb != null)
        {
            savedWorldPos = new Vector3(rb.position.x, rb.position.y, transform.position.z);
        }
        else
        {
            savedWorldPos = transform.position;
        }

        // --- Effectue le flip en conservant la magnitude de l'échelle X ---
        Vector3 s = GFX_Container.localScale;
        float absX = Mathf.Abs(s.x);
        GFX_Container.localScale = new Vector3(absX * desiredSign, s.y, s.z);

        // --- Restauration des positions/rotations/vitesses des rigidbodies et réactivation ---
        for (int i = 0; i < childRbs.Length; i++)
        {
            // repositionne via Rigidbody2D pour respecter la physique
            childRbs[i].position = savedPositions[i];
            childRbs[i].rotation = savedRotations[i];
            childRbs[i].linearVelocity = savedVelocities[i];

            childRbs[i].simulated = true;
            childRbs[i].WakeUp();
        }

        // --- Restauration de la position principale du joueur ---
        if (rb != null)
        {
            rb.position = new Vector2(savedWorldPos.x, savedWorldPos.y);
        }
        else
        {
            transform.position = savedWorldPos;
        }
    }
}