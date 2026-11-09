using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }
    public PlayerID playerType;

    [Header("Movement & Jump")]
    public float moveSpeed = 8f;
    public float jumpForce = 13f;
    public int maxJumps = 2;              // 2 = Double Jump allow karega
    private int jumpsRemaining;           // Bachi hui jumps track karega
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Push Settings")]
    public float pushDistance = 2.0f;     
    public float pushImpulse = 20f;      
    public float knockbackDuration = 0.3f; 

    [Header("Respawn")]
    public float respawnDelay = 2.0f;
    private Vector3 spawnPosition;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator anim;
    private float baseScaleX;
    private bool isGrounded;
    private bool isDead = false;
    private bool isKnockedBack = false;
    private float horizontalInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        spawnPosition = transform.position;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Start()
    {
        ResetFacingDirection();
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGameOver)) 
            return;

        ReadInput();

        // 1. Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 2. Zameen par aate hi jumps reset hongi
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // 3. Jump & Double Jump Logic
        if (!isKnockedBack && GetJumpInput())
        {
            // Agar zameen par ho YA hawa mein jumps bachi hon
            if (isGrounded || jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--; // Ek jump kam ho gayi
            }
        }

        // Push input
        if (!isKnockedBack && GetPushInput())
        {
            PushOpponent();
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGameOver)) 
            return;

        if (!isKnockedBack)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        bool running = Mathf.Abs(horizontalInput) > 0.1f && isGrounded;
        anim.SetBool("isRunning", running);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void ReadInput()
    {
        if (playerType == PlayerID.Player1)
        {
            horizontalInput = 0f;
            if (Input.GetKey(KeyCode.A)) { horizontalInput -= 1f; }
            if (Input.GetKey(KeyCode.D)) { horizontalInput += 1f; }
        }
        else
        {
            horizontalInput = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) { horizontalInput -= 1f; }
            if (Input.GetKey(KeyCode.RightArrow)) { horizontalInput += 1f; }
        }

        if (horizontalInput > 0f) SetFacingRight(true);
        else if (horizontalInput < 0f) SetFacingRight(false);
    }

    private void SetFacingRight(bool faceRight)
    {
        float scaleX = faceRight ? baseScaleX : -baseScaleX;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    private void ResetFacingDirection()
    {
        if (playerType == PlayerID.Player1) SetFacingRight(true);
        else SetFacingRight(false);
    }

    private bool GetJumpInput()
    {
        return playerType == PlayerID.Player1 ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.UpArrow);
    }

    private bool GetPushInput()
    {
        if (playerType == PlayerID.Player1)
        {
            return Input.GetKeyDown(KeyCode.F);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Keypad0);
        }
    }

    private void PushOpponent()
    {
        if (anim != null)
        {
            anim.SetTrigger("push");
        }

        PlayerController[] allPlayers = Object.FindObjectsOfType<PlayerController>();
        foreach (var other in allPlayers)
        {
            if (other != this && !other.isDead)
            {
                float dist = Vector2.Distance(transform.position, other.transform.position);

                if (dist <= pushDistance)
                {
                    Vector2 pushDir = (other.transform.position - transform.position).normalized;
                    pushDir.y = Mathf.Clamp(pushDir.y + 0.5f, 0.4f, 0.9f);
                    pushDir.Normalize();

                    other.TakeKnockback(pushDir * pushImpulse, knockbackDuration);
                }
            }
        }
    }

    public void TakeKnockback(Vector2 force, float duration)
    {
        if (isDead) return;
        StartCoroutine(KnockbackRoutine(force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    public void Eliminate()
    {
        if (isDead) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isDead = true;
        isKnockedBack = false;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled = false;
        sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPosition;
        ResetFacingDirection();
        jumpsRemaining = maxJumps; // Respawn par jumps wapas reset

        rb.isKinematic = false;
        col.enabled = true;
        sr.enabled = true;
        isDead = false;
    }
}