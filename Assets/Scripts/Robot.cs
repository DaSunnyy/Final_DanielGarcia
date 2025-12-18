using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Robot : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float moveDistance = 3f;
    public float pauseTime = 2f;

    [Header("Detection & Shooting")]
    public float detectionRadius = 6f;
    public GameObject bulletPrefab;
    public float fireCooldown = 2f;

    private Rigidbody2D rb;
    private Vector2 targetPos;

    private bool isMoving = true;
    private float pauseTimer = 0f;
    private float fireTimer = 0f;

    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        targetPos = (Vector2)transform.position + Vector2.right * moveDistance;

        transform.localScale = new Vector3(-10f, 10f, 1f);
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        FlipSprite();
    }

    void HandleMovement()
    {
        if (isMoving)
        {
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);

            if (Vector2.Distance(transform.position, targetPos) < 0.05f)
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                pauseTimer = 0f;
            }
        }
        else
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseTime)
            {
                float newDir = targetPos.x > transform.position.x ? -1f : 1f;
                targetPos = (Vector2)transform.position + Vector2.right * newDir * moveDistance;
                isMoving = true;
            }
        }
    }

    void HandleShooting()
    {
        if (!player || !bulletPrefab) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRadius) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireCooldown)
        {
            fireTimer = 0f;
            StartCoroutine(ShootTwoBulletsCoroutine());
        }
    }

    IEnumerator ShootTwoBulletsCoroutine()
    {
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        ShootBullet(dirToPlayer);      
        yield return new WaitForSeconds(1f);
        ShootBullet(dirToPlayer);      
    }

    void ShootBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetDirection(direction);
    }

    void FlipSprite()
    {
        if (!player) return;

        float scaleX = transform.localScale.x;

        if (player.position.x > transform.position.x && scaleX < 0)
            transform.localScale = new Vector3(10f, 10f, 1f);

        else if (player.position.x < transform.position.x && scaleX > 0)
            transform.localScale = new Vector3(-10f, 10f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}