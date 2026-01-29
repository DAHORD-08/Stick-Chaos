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
        _isOnGround = Physics2D.OverlapCircle(playerPos.position, positionRadius, ground);

        // Input.GetAxisRaw("Vertical") > 0 gère Z, W et la flèche haut simultanément
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("Vertical") > 0;

        if (_isOnGround && jumpPressed)
        {
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
}