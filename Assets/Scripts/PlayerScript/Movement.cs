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
    [SerializeField] private SpriteRenderer headSprite;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float stepWait = 0.5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Collision Settings")]
    [SerializeField] private float positionRadius = 0.2f;
    [SerializeField] private LayerMask ground;

    [Header("Arm Components")]
    [SerializeField] private ArmController leftArmScript;
    [SerializeField] private ArmController rightArmScript;

    private float _lastClickTime;
    private const float DoubleClickTimeThreshold = 0.25f;
    private bool _bothArmsActive = false;

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
        HandleArmInput();
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            if (headSprite != null)
            {
                headSprite.flipX = (horizontalInput < 0);
            }

            if (!_isWalking)
            {
                string animationName = horizontalInput > 0 ? "WalkRight" : "WalkLeft";
                anim.Play(animationName);
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
        // 1. Détection précise du sol
        _isOnGround = Physics2D.OverlapCircle(playerPos.position, positionRadius, ground);

        // 2. Détection de l'intention de saut (Appui initial uniquement)
        bool jumpKeyPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.W);

        if (_isOnGround && jumpKeyPressed)
        {
            // 3. Application d'une force d'impulsion unique
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void HandleArmInput()
    {
        // Détection double-clic (Bouton Gauche)
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - _lastClickTime < DoubleClickTimeThreshold)
            {
                _bothArmsActive = true;
            }
            _lastClickTime = Time.time;
        }

        // Arrêt si aucun bouton n'est pressé
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            _bothArmsActive = false;
            leftArmScript.isActive = false;
            rightArmScript.isActive = false;
            return;
        }

        // Activation dynamique
        if (_bothArmsActive)
        {
            leftArmScript.isActive = true;
            rightArmScript.isActive = true;
        }
        else
        {
            leftArmScript.isActive = Input.GetMouseButton(0);
            rightArmScript.isActive = Input.GetMouseButton(1);
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
}