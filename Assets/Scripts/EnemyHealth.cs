using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Hit Feedback")]
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Attack Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpForce = 2f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Body Push")]
    [SerializeField] private float bodyPushDistance = 0.18f;
    [SerializeField] private float bodyPushDuration = 0.12f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private Color originalColor;

    private Coroutine bodyPushCoroutine;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor =
                spriteRenderer.color;
        }

        // Enemy 平时默认锁住 X
        LockHorizontalMovement();
    }

    // -------------------------
    // 受到攻击
    // -------------------------
    public void TakeDamage(
        int damage,
        Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} took {damage} damage. HP: {currentHealth}"
        );

        if (rb != null)
        {
            // 攻击优先于普通 Body Push
            if (bodyPushCoroutine != null)
            {
                StopCoroutine(
                    bodyPushCoroutine
                );

                bodyPushCoroutine = null;
            }

            if (knockbackCoroutine != null)
            {
                StopCoroutine(
                    knockbackCoroutine
                );
            }

            knockbackCoroutine =
                StartCoroutine(
                    Knockback(
                        knockbackDirection
                    )
                );
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(
                HitFlash()
            );
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // -------------------------
    // 受击闪红
    // -------------------------
    private IEnumerator HitFlash()
    {
        spriteRenderer.color =
            Color.red;

        yield return new WaitForSeconds(
            hitFlashDuration
        );

        spriteRenderer.color =
            originalColor;
    }

    // -------------------------
    // J 攻击造成的正式击退
    // -------------------------
    private IEnumerator Knockback(
        Vector2 knockbackDirection)
    {
        // 临时解除 X 锁
        rb.constraints =
            RigidbodyConstraints2D
                .FreezeRotation;

        rb.linearVelocity =
            new Vector2(
                knockbackDirection.x *
                knockbackForce,

                knockbackUpForce
            );

        yield return new WaitForSeconds(
            knockbackDuration
        );

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );

        // 攻击结束重新锁住 X
        LockHorizontalMovement();

        knockbackCoroutine = null;
    }

    // -------------------------
    // Player 身体撞击
    // -------------------------
    public bool TryBodyPush(
        int direction)
    {
        if (rb == null ||
            bodyPushCoroutine != null ||
            knockbackCoroutine != null)
        {
            return false;
        }

        bodyPushCoroutine =
            StartCoroutine(
                BodyPush(direction)
            );

        return true;
    }

    // -------------------------
    // 身体碰撞的小幅移动
    // -------------------------
    private IEnumerator BodyPush(
        int direction)
    {
        float startX =
            rb.position.x;

        float targetX =
            startX +
            direction *
            bodyPushDistance;

        float elapsedTime = 0f;

        // Body Push 期间临时解除 X
        rb.constraints =
            RigidbodyConstraints2D
                .FreezeRotation;

        while (elapsedTime <
               bodyPushDuration)
        {
            elapsedTime +=
                Time.fixedDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedTime /
                    bodyPushDuration
                );

            float newX =
                Mathf.Lerp(
                    startX,
                    targetX,
                    t
                );

            rb.MovePosition(
                new Vector2(
                    newX,
                    rb.position.y
                )
            );

            yield return
                new WaitForFixedUpdate();
        }

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );

        // 挪完以后重新锁住 X
        LockHorizontalMovement();

        bodyPushCoroutine = null;
    }

    // -------------------------
    // Player 真正离开 Enemy 后
    // 允许以后再次 Body Push
    // -------------------------
    public bool ReleaseBodyPushLock()
    {
        if (rb == null)
        {
            return false;
        }

        // 攻击 Knockback 还没结束的话
        // 暂时不要释放
        if (knockbackCoroutine != null ||
            bodyPushCoroutine != null)
        {
            return false;
        }

        // 物理 X 仍然保持锁定。
        // 下一次 BodyPush 会自己临时解锁。
        LockHorizontalMovement();

        return true;
    }

    private void LockHorizontalMovement()
    {
        if (rb == null)
        {
            return;
        }

        rb.constraints =
            RigidbodyConstraints2D
                .FreezePositionX |
            RigidbodyConstraints2D
                .FreezeRotation;
    }

    private void Die()
    {
        Debug.Log(
            $"{gameObject.name} defeated."
        );

        Destroy(gameObject);
    }
}