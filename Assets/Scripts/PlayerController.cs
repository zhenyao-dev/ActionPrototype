using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackOffset = 0.8f;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Body Push")]
    [SerializeField] private float bodyPushCooldown = 0.2f;
    [SerializeField] private float bodyPushResetDistance = 0.1f;

    private EnemyHealth bodyPushLockedEnemy;
    private Collider2D bodyPushLockedCollider;
    private Collider2D playerCollider;

    private int bodyPushDirection;
    private float bodyPushReadyTime;

    private Rigidbody2D rb;

    private float moveInput;
    private bool isGrounded;
    private int facingDirection = 1;

    private float nextAttackTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        moveInput = 0f;

        if (Keyboard.current == null)
        {
            return;
        }

        // -------------------------
        // 左右移动输入
        // -------------------------
        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput += 1f;
        }

        // -------------------------
        // 记录角色朝向
        // -------------------------
        if (moveInput > 0f)
        {
            facingDirection = 1;
        }
        else if (moveInput < 0f)
        {
            facingDirection = -1;
        }

        // AttackPoint 跟随朝向切换左右
        if (attackPoint != null)
        {
            attackPoint.localPosition = new Vector3(
                attackOffset * facingDirection,
                0f,
                0f
            );
        }

        // -------------------------
        // 地面检测
        // -------------------------
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        // -------------------------
        // 跳跃
        // -------------------------
        if (Keyboard.current.spaceKey.wasPressedThisFrame &&
            isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        // -------------------------
        // 攻击
        // -------------------------
        if (Keyboard.current.jKey.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Attack();

            nextAttackTime =
                Time.time + attackCooldown;
        }

        if (bodyPushLockedEnemy != null &&
            bodyPushLockedCollider != null)
        {
            float horizontalGap;

            if (bodyPushDirection == 1)
            {
                // Enemy 在右边
                horizontalGap =
                    bodyPushLockedCollider.bounds.min.x -
                    playerCollider.bounds.max.x;
            }
            else
            {
                // Enemy 在左边
                horizontalGap =
                    playerCollider.bounds.min.x -
                    bodyPushLockedCollider.bounds.max.x;
            }

            bool movingAway =
                (bodyPushDirection == 1 && moveInput < 0f) ||
                (bodyPushDirection == -1 && moveInput > 0f);

            bool cooldownFinished =
                Time.time >= bodyPushReadyTime;

            bool farEnough =
                horizontalGap >= bodyPushResetDistance;

            // 必须：
            // 1. 冷却结束
            // 2. 主动往反方向走
            // 3. 真的拉开距离
            if (cooldownFinished &&
                movingAway &&
                farEnough)
            {
                bodyPushLockedEnemy.ReleaseBodyPushLock();

                bodyPushLockedEnemy = null;
                bodyPushLockedCollider = null;
                bodyPushDirection = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // -------------------------
    // 身体撞到 Enemy
    // -------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 只处理 Enemy Layer
        if (collision.gameObject.layer !=
            LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        // 上一次还没真正解除，就不能再次推
        if (bodyPushLockedEnemy != null)
        {
            return;
        }

        EnemyHealth enemyHealth =
            collision.gameObject.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        float horizontalSpeed =
            rb.linearVelocity.x;

        // 必须是真的带着水平速度撞过去
        if (Mathf.Abs(horizontalSpeed) < 0.1f)
        {
            return;
        }

        int direction =
            horizontalSpeed > 0f ? 1 : -1;

        // 确认 Enemy 位于玩家移动方向前方
        float enemyDirection =
            collision.transform.position.x -
            transform.position.x;

        if (enemyDirection * direction <= 0f)
        {
            return;
        }

        // 让 Enemy 贴着玩家稍微挪动一点
        enemyHealth.TryBodyPush(direction);

        // 记录这次已经推过
        bodyPushLockedEnemy = enemyHealth;

        bodyPushLockedCollider =
            collision.gameObject.GetComponent<Collider2D>();

        bodyPushDirection = direction;

        // 只是记录“最早何时允许下一次”
        bodyPushReadyTime =
            Time.time + bodyPushCooldown;
    }

    // -------------------------
    // 攻击判定
    // -------------------------
    private void Attack()
    {
        if (attackPoint == null)
        {
            return;
        }

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                enemyLayer
            );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Vector2 knockbackDirection =
                    new Vector2(
                        facingDirection,
                        0f
                    );

                enemyHealth.TakeDamage(
                    attackDamage,
                    knockbackDirection
                );
            }
        }
    }

    // -------------------------
    // Scene 中显示检测范围
    // -------------------------
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRange
            );
        }
    }
}