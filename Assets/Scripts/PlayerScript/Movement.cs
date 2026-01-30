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
        // On ne change le scale que si nécessaire pour éviter les calculs inutiles
        if (GFX_Container == null) return;

        float currentX = GFX_Container.localScale.x;
        float desiredSign = direction > 0 ? 1f : -1f;

        // Si le signe est déjà celui désiré, rien à faire
        if (Mathf.Sign(currentX) == desiredSign) return;

        // --- Sauvegarde de la position avant flip ---
        Vector3 savedWorldPos;
        if (rb != null)
        {
            // Rigidbody2D utilise Vector2, on convertit en Vector3 pour garder la z
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

        // --- Restauration de la position après flip ---
        if (rb != null)
        {
            // On repositionne le Rigidbody (évite conflits physiques avec transform)
            rb.position = new Vector2(savedWorldPos.x, savedWorldPos.y);
            // Si nécessaire, on peut aussi réinitialiser la vitesse verticale/horizontale, ex:
            // rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        else
        {
            transform.position = savedWorldPos;
        }
    }
}