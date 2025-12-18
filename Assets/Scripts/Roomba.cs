using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Roomba : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveDuration = 5f;
    public float waitDuration = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool movingRight = true;
    private bool isWaiting = false;
    private float moveTimer = 0f;
    private float waitTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.localScale = new Vector3(4.05f, 4.05f, 1f);
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (waitTimer >= waitDuration)
            {
                isWaiting = false;
                waitTimer = 0f;
                movingRight = !movingRight;
            }
        }
        else
        {
            moveTimer += Time.deltaTime;
            rb.linearVelocity = new Vector2((movingRight ? 1f : -1f) * moveSpeed, rb.linearVelocity.y);

            if (spriteRenderer)
                spriteRenderer.flipX = !movingRight;

            if (moveTimer >= moveDuration)
            {
                isWaiting = true;
                moveTimer = 0f;
            }
        }
    }
}