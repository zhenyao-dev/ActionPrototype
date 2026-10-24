using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Detection")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyHealth enemyHealth;

    private int direction = -1;

    private float wallCheckDistance;
    private float groundCheckDistance;

    private bool canTurn = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }

        // 记录两个检测点距离 Enemy 中心有多远
        if (wallCheck != null)
        {
            wallCheckDistance =
                Mathf.Abs(wallCheck.localPosition.x);
        }

        if (groundCheck != null)
        {
            groundCheckDistance =
                Mathf.Abs(groundCheck.localPosition.x);
        }

        UpdateCheckPositions();
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        // 正在受击或被身体推动时，巡逻暂时停止接管
        if (enemyHealth != null &&
            enemyHealth.IsMovementLocked)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            return;
        }

        bool wallAhead = false;
        bool groundAhead = true;

        // 前方墙壁检测
        if (wallCheck != null)
        {
            wallAhead =
                Physics2D.OverlapCircle(
                    wallCheck.position,
                    checkRadius,
                    groundLayer
                ) != null;
        }

        // 前下方地面检测
        if (groundCheck != null)
        {
            groundAhead =
                Physics2D.OverlapCircle(
                    groundCheck.position,
                    checkRadius,
                    groundLayer
                ) != null;
        }

        // 撞墙 or 前面没地面 → 掉头
        if ((wallAhead || !groundAhead) && canTurn)
        {
            TurnAround();
            canTurn = false;
        }

        // 检测恢复正常后，才允许下一次掉头
        if (!wallAhead && groundAhead)
        {
            canTurn = true;
        }

        rb.linearVelocity =
            new Vector2(
                direction * moveSpeed,
                rb.linearVelocity.y
            );

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                Mathf.Abs(rb.linearVelocity.x)
            );
        }
    }

    private void TurnAround()
    {
        direction *= -1;

        UpdateFacing();
        UpdateCheckPositions();
    }

    private void UpdateFacing()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX =
                direction > 0;
        }
    }

    private void UpdateCheckPositions()
    {
        if (wallCheck != null)
        {
            Vector3 position =
                wallCheck.localPosition;

            position.x =
                wallCheckDistance * direction;

            wallCheck.localPosition =
                position;
        }

        if (groundCheck != null)
        {
            Vector3 position =
                groundCheck.localPosition;

            position.x =
                groundCheckDistance * direction;

            groundCheck.localPosition =
                position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (wallCheck != null)
        {
            Gizmos.DrawWireSphere(
                wallCheck.position,
                checkRadius
            );
        }

        Gizmos.color = Color.green;

        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                checkRadius
            );
        }
    }
}