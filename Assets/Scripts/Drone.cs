using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Drone : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 4f;
    public float moveSpeed = 6f;
    public float waitTime = 4f;

    [Header("Detection & Shooting")]
    public float detectionRadius = 6f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireCooldown = 5f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving = true;
    private float waitTimer = 0f;
    private float fireTimer = 0f;

    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = (Vector2)transform.position + RandomDirection() * moveDistance;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        transform.localScale = new Vector3(8f, 8f, 1f);
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        FlipTowardsMovement();
    }

    Vector2 RandomDirection()
    {
        int dir = Random.Range(0, 4);
        switch (dir)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            default: return Vector2.right;
        }
    }

    void HandleMovement()
    {
        if (isMoving)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                waitTimer = 0f;
            }
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                targetPosition = (Vector2)transform.position + RandomDirection() * moveDistance;
                isMoving = true;
            }
        }
    }

    void HandleShooting()
    {
        if (!player) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireCooldown)
            {
                fireTimer = 0f;
                ShootAtPlayer();
            }
        }
    }

    void ShootAtPlayer()
    {
        if (!bulletPrefab || !player) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            bulletScript.SetDirection(dirToPlayer);
        }
    }

    void FlipTowardsMovement()
    {
        if (rb.linearVelocity.x > 0)
            transform.localScale = new Vector3(8f, 8f, 1f);
        else if (rb.linearVelocity.x < 0)
            transform.localScale = new Vector3(-8f, 8f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
    }
}