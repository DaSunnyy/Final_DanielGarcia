using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.2f;
    public float runMultiplier = 1.3f;
    public float jumpForce = 3.3f;
    public float jumpTime = 0.35f;
    public float fallMultiplier = 2f;
    public float lowJumpMultiplier = 2f;

    [Header("Crouch Settings")]
    public float crouchHeightMultiplier = 0.5f;

    [Header("Climb Settings")]
    public float climbSpeed = 3f;

    [Header("Magnet Settings")]
    public Transform MagnetPoint;
    public float attractRange = 6f;
    public float attractSpeed = 5f;
    public float shootForce = 12f;
    public LayerMask metalLayer;

    [Header("References")]
    public Animator anim;

    [HideInInspector] public Rigidbody2D rb;

    private BoxCollider2D boxCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector2 crouchColliderSize;
    private Vector2 crouchColliderOffset;

    private float jumpTimer;
    private float verticalInput;
    private float normalGravity;

    private GameObject heldMetal;
    private Rigidbody2D heldRb;
    private Collider2D heldCol;

    public bool onLadder;
    public bool isGrounded;
    public bool isCrouching;
    public bool isClimbing;
    public bool useMagnet;
    public bool LookUp;
    public bool LookUpMAG;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        normalGravity = rb.gravityScale;

        if (anim == null)
            anim = GetComponent<Animator>();

        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;

        crouchColliderSize = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchHeightMultiplier);
        crouchColliderOffset = new Vector2(originalColliderOffset.x,
            originalColliderOffset.y - (originalColliderSize.y - crouchColliderSize.y) / 2f);
    }

    void Update()
    {
        HandleInput();
        BetterGravity();
        HandleClimb();
        HandleMagnetInput();
        MoveHeldMetal();
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.S) && isGrounded && !onLadder)
        {
            if (!isCrouching) SetCrouch(true);
        }
        else
        {
            if (isCrouching) SetCrouch(false);
        }

        float move = 0f;
        if (!isCrouching && !isClimbing)
        {
            if (Input.GetKey(KeyCode.A)) move = -1f;
            if (Input.GetKey(KeyCode.D)) move = 1f;
        }

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.J)) speed *= runMultiplier;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (move > 0) transform.localScale = new Vector3(1.5f, 1.5f, 1);
        else if (move < 0) transform.localScale = new Vector3(-1.5f, 1.5f, 1);

        if (!isCrouching && !isClimbing)
        {
            if (Input.GetKeyDown(KeyCode.K) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimer = jumpTime;
                isGrounded = false;

                SFXManager.Instance.PlaySFX(SFXManager.Instance.Jump);
            }

            if (Input.GetKey(KeyCode.K) && jumpTimer > 0)
            {
                rb.linearVelocity += Vector2.up * (jumpForce * Time.deltaTime);
                jumpTimer -= Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.K)) jumpTimer = 0;
        }

        LookUp = Input.GetKey(KeyCode.W) && !isClimbing && !isCrouching;
    }

    void SetCrouch(bool crouch)
    {
        if (crouch)
        {
            boxCollider.size = crouchColliderSize;
            boxCollider.offset = crouchColliderOffset;
            float bottomDelta = (originalColliderSize.y - crouchColliderSize.y) / 2f;
            rb.position = new Vector2(rb.position.x, rb.position.y - bottomDelta);
            isCrouching = true;
        }
        else
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
            float bottomDelta = (originalColliderSize.y - crouchColliderSize.y) / 2f;
            rb.position = new Vector2(rb.position.x, rb.position.y + bottomDelta);
            isCrouching = false;
        }
    }

    void BetterGravity()
    {
        if (!isClimbing)
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.K))
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void HandleClimb()
    {
        verticalInput = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        if (onLadder && Mathf.Abs(verticalInput) > 0)
        {
            if (!isClimbing)
            {
                isClimbing = true;
                anim?.Play("Player_Climb", 0, 0f);
            }
        }
        else if (!onLadder) isClimbing = false;

        if (Input.GetKeyDown(KeyCode.K) && isClimbing)
        {
            isClimbing = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
        }
        else
            rb.gravityScale = normalGravity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
            onLadder = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            onLadder = false;
            isClimbing = false;
        }
    }
    private bool magnetPlaying = false;

    void HandleMagnetInput()
    {
        bool attractHeld = Input.GetKey(KeyCode.L);
        bool shootPressed = Input.GetKeyDown(KeyCode.Space);

        useMagnet = attractHeld || (heldMetal != null && shootPressed);
        LookUpMAG = LookUp && useMagnet;

        if (attractHeld && heldMetal == null)
        {
            if (LookUp)
                AttractVertical();
            else
                AttractHorizontal();

            if (!magnetPlaying)
            {
                SFXManager.Instance.PlayLoop(SFXManager.Instance.MagnetAttract);
                magnetPlaying = true;
            }
        }
        else
        {
            if (magnetPlaying)
            {
                SFXManager.Instance.StopLoop(SFXManager.Instance.MagnetAttract);
                magnetPlaying = false;
            }
        }

        if (heldMetal != null && shootPressed)
        {
            Shoot();
            SFXManager.Instance.PlaySFX(SFXManager.Instance.MagnetShot);
        }
    }

    void AttractHorizontal()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attractRange * 0.5f;
        Vector2 size = new Vector2(attractRange, 1f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Metal") && heldMetal == null)
            {
                Grab(hit);
                break;
            }
        }
    }

    void AttractVertical()
    {
        Vector2 center = (Vector2)MagnetPoint.position + Vector2.up * attractRange * 0.5f;
        Vector2 size = new Vector2(1f, attractRange);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Metal") && heldMetal == null)
            {
                Grab(hit);
                break;
            }
        }
    }

    void Grab(Collider2D hit)
    {
        Rigidbody2D rbHit = hit.attachedRigidbody;
        if (!rbHit || rbHit.bodyType == RigidbodyType2D.Static) return;

        heldMetal = hit.gameObject;
        heldRb = rbHit;
        heldCol = hit;

        heldRb.linearVelocity = Vector2.zero;
        heldRb.gravityScale = 0f;
        heldCol.enabled = false;
    }

    void MoveHeldMetal()
    {
        if (heldMetal == null || heldRb == null) return;

        Vector2 target = MagnetPoint.position;
        Vector2 direction = target - heldRb.position;
        float distance = direction.magnitude;

        if (distance < 0.1f)
            heldRb.MovePosition(target);
        else
        {
            Vector2 move = direction.normalized * attractSpeed * Time.deltaTime;
            if (move.magnitude > distance) move = direction;
            heldRb.MovePosition(heldRb.position + move);
        }
    }

    void Shoot()
    {
        if (heldMetal == null) return;

        heldCol.enabled = true;
        heldCol.isTrigger = false;

        heldRb.gravityScale = normalGravity;
        heldRb.bodyType = RigidbodyType2D.Dynamic;
        heldRb.linearVelocity = Vector2.zero;

        BoxDamage boxDamage = heldMetal.GetComponent<BoxDamage>();
        if (!boxDamage)
            boxDamage = heldMetal.AddComponent<BoxDamage>();

        boxDamage.damage = 1;

        Vector2 dir = LookUp ? Vector2.up : new Vector2(Mathf.Sign(transform.localScale.x), 0f);

        heldRb.AddForce(dir * shootForce, ForceMode2D.Impulse);

        heldMetal = null;
        heldRb = null;
        heldCol = null;
    }

    void OnDrawGizmosSelected()
    {
        if (!MagnetPoint) return;

        float facing = transform.localScale.x > 0 ? 1 : -1;

        Gizmos.color = Color.blue;
        Vector2 centerH = (Vector2)transform.position + Vector2.right * facing * attractRange * 0.5f;
        Vector2 sizeH = new Vector2(attractRange, 1f);
        Gizmos.DrawWireCube(centerH, sizeH);

        Gizmos.color = Color.green;
        Vector2 centerV = (Vector2)MagnetPoint.position + Vector2.up * attractRange * 0.5f;
        Vector2 sizeV = new Vector2(1f, attractRange);
        Gizmos.DrawWireCube(centerV, sizeV);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}