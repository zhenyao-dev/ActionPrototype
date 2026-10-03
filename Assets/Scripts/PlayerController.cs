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

    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private float moveInput;
    private bool isGrounded;
    private int facingDirection = 1;

    private float nextAttackTime = 0f;

    // Body Push 状态
    private EnemyHealth bodyPushLockedEnemy;
    private Collider2D bodyPushLockedCollider;
    private float bodyPushReadyTime;

    // 受伤硬直
    private bool isHurt;
    private float hurtEndTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        moveInput = 0f;

        // -------------------------
        // 受伤硬直结束
        // -------------------------
        if (isHurt && Time.time >= hurtEndTime)
        {
            isHurt = false;
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
        // AttackPoint 跟随朝向
        // -------------------------
        if (attackPoint != null)
        {
            attackPoint.localPosition = new Vector3(
                attackOffset * facingDirection,
                0f,
                0f
            );
        }

        // -------------------------
        // 正常控制
        // 受伤硬直期间不能操作
        // -------------------------
        if (!isHurt && Keyboard.current != null)
        {
            // 左
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput -= 1f;
            }

            // 右
            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput += 1f;
            }

            // 更新朝向
            if (moveInput > 0f)
            {
                facingDirection = 1;
            }
            else if (moveInput < 0f)
            {
                facingDirection = -1;
            }

            // 跳跃
            if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );
            }

            // J 键攻击
            if (Keyboard.current.jKey.wasPressedThisFrame &&
                Time.time >= nextAttackTime)
            {
                Attack();

                nextAttackTime =
                    Time.time + attackCooldown;
            }
        }

        // Body Push 的解除检测
        UpdateBodyPushLock();
    }

    private void FixedUpdate()
    {
        // 受伤硬直期间不能用移动输入覆盖击退速度
        if (isHurt)
        {
            return;
        }

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // -------------------------
    // Player 受到伤害时的击退 + 硬直
    // -------------------------
    public void ApplyDamageKnockback(
        Vector2 velocity,
        float hurtDuration)
    {
        isHurt = true;

        hurtEndTime =
            Time.time + hurtDuration;

        moveInput = 0f;

        rb.linearVelocity = velocity;
    }

    // -------------------------
    // 身体撞到 Enemy
    // -------------------------
    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        // 受伤被弹飞期间不触发 Body Push
        if (isHurt)
        {
            return;
        }

        if (collision.gameObject.layer !=
            LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        // 上一次 Body Push 还没解除
        if (bodyPushLockedEnemy != null)
        {
            return;
        }

        EnemyHealth enemyHealth =
            collision.gameObject
                .GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        // 只接受侧面碰撞
        bool isSideCollision = false;

        foreach (ContactPoint2D contact
                 in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isSideCollision = true;
                break;
            }
        }

        if (!isSideCollision)
        {
            return;
        }

        // Enemy 位于 Player 哪一侧
        int direction =
            collision.transform.position.x >
            transform.position.x
            ? 1
            : -1;

        // Enemy 当前如果正在 Knockback 等，
        // TryBodyPush 会返回 false
        bool started =
            enemyHealth.TryBodyPush(direction);

        if (!started)
        {
            return;
        }

        bodyPushLockedEnemy = enemyHealth;

        bodyPushLockedCollider =
            collision.collider;

        bodyPushReadyTime =
            Time.time + bodyPushCooldown;
    }

    // -------------------------
    // 检查是否允许下一次 Body Push
    // -------------------------
    private void UpdateBodyPushLock()
    {
        if (bodyPushLockedEnemy == null)
        {
            bodyPushLockedCollider = null;
            return;
        }

        if (bodyPushLockedCollider == null ||
            playerCollider == null)
        {
            bodyPushLockedEnemy = null;
            bodyPushLockedCollider = null;
            return;
        }

        // 冷却还没结束
        if (Time.time < bodyPushReadyTime)
        {
            return;
        }

        // 直接问 Unity：
        // Player 和这个 Enemy 现在还碰没碰着
        bool stillTouching =
            playerCollider.IsTouching(
                bodyPushLockedCollider
            );

        // 还贴着就绝对不能再次推
        if (stillTouching)
        {
            return;
        }

        bool released =
            bodyPushLockedEnemy
                .ReleaseBodyPushLock();

        // Enemy 如果还处于攻击 Knockback，
        // 本帧解除失败，下帧继续尝试
        if (!released)
        {
            return;
        }

        bodyPushLockedEnemy = null;
        bodyPushLockedCollider = null;
    }

    // -------------------------
    // 攻击
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

            if (enemyHealth == null)
            {
                continue;
            }

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

    // -------------------------
    // Scene 中显示判定范围
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