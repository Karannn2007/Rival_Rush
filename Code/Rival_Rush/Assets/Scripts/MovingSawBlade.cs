using UnityEngine;

public class MovingSawBlade : MonoBehaviour
{
    [Header("Patrol Limits (Start position se kitna door jana hai)")]
    public float leftDistance = 3f;   // Start position se kitna left jayega
    public float rightDistance = 3f;  // Start position se kitna right jayega
    public float moveSpeed = 3f;      // Chalne ki speed

    [Header("Blade Spinning")]
    public bool enableSpin = true;
    public float spinSpeed = 400f;    // Cutter ghoomne ki speed

    private Vector3 startPosition;
    private Vector3 leftTarget;
    private Vector3 rightTarget;
    private bool movingToRight = true;

    void Start()
    {
        // Jahan aapne blade rakhi hai, wo iska center point hoga
        startPosition = transform.position;
        leftTarget = startPosition + Vector3.left * leftDistance;
        rightTarget = startPosition + Vector3.right * rightDistance;
    }

    void Update()
    {
        // 1. Blade ko ghumana (Spin effect)
        if (enableSpin)
        {
            transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
        }

        // 2. Left aur Right patrol movement
        Vector3 target = movingToRight ? rightTarget : leftTarget;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Kinare pahunchne par disha badalna
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            movingToRight = !movingToRight;
        }
    }

    // Player touch hone par eliminate karna
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.Eliminate();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.Eliminate();
        }
    }

    // Scene View mein dekhne ke liye (Red Line dikhegi ki blade kahan tak jayegi)
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.left * leftDistance, center + Vector3.right * rightDistance);
        Gizmos.DrawWireSphere(center + Vector3.left * leftDistance, 0.2f);
        Gizmos.DrawWireSphere(center + Vector3.right * rightDistance, 0.2f);
    }
}