using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float patrolDistance = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyHealth enemyHealth;

    private float startX;
    private int direction = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        startX = transform.position.x;

        // 巡逻敌人只锁旋转，不锁 X
        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        // 正在受击或被身体推动时，
        // 暂时让 EnemyHealth 接管移动
        if (enemyHealth != null &&
            enemyHealth.IsMovementLocked)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            return;
        }

        float leftLimit =
            startX - patrolDistance;

        float rightLimit =
            startX + patrolDistance;

        if (transform.position.x <= leftLimit)
        {
            direction = 1;
        }
        else if (transform.position.x >= rightLimit)
        {
            direction = -1;
        }

        rb.linearVelocity =
            new Vector2(
                direction * moveSpeed,
                rb.linearVelocity.y
            );

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX =
                direction > 0;
        }

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                Mathf.Abs(rb.linearVelocity.x)
            );
        }
    }
}