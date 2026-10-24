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

    [Header("Death")]
    [SerializeField] private float deathDuration = 0.35f;
    [SerializeField] private float deathUpForce = 2.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private Color originalColor;

    private Coroutine bodyPushCoroutine;
    private Coroutine knockbackCoroutine;

    private bool isDead;

    private Collider2D enemyCollider;
    private EnemyPatrol enemyPatrol;
    private EnemyContactDamage contactDamage;

    public bool IsMovementLocked =>
        bodyPushCoroutine != null ||
        knockbackCoroutine != null;

    private void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        enemyPatrol = GetComponent<EnemyPatrol>();
        contactDamage = GetComponent<EnemyContactDamage>();

        if (spriteRenderer != null)
        {
            originalColor =
                spriteRenderer.color;
        }
    }

    // -------------------------
    // 受到攻击
    // -------------------------
    public void TakeDamage(
        int damage,
        Vector2 knockbackDirection)
    {
        if (isDead)
        {
            return;
        }
        
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
        // 击退期间由 EnemyHealth 控制移动
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

        // Body Push 期间由 EnemyHealth 控制移动
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

        return true;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log(
            $"{gameObject.name} defeated."
        );

        // 停止当前受击 / Body Push Coroutine
        StopAllCoroutines();

        bodyPushCoroutine = null;
        knockbackCoroutine = null;

        // 死亡以后不再巡逻
        if (enemyPatrol != null)
        {
            enemyPatrol.enabled = false;
        }

        // 死亡以后不能再伤害 Player
        if (contactDamage != null)
        {
            contactDamage.enabled = false;
        }

        // 不再参与碰撞
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // 恢复原来的颜色，避免死的时候卡在红色
        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                originalColor;
        }

        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;

            // 保留最后一击的横向惯性，再向上弹一下
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    deathUpForce
                );
        }

        StartCoroutine(
            DeathRoutine()
        );
    }

    private IEnumerator DeathRoutine()
    {
        float elapsedTime = 0f;

        Vector3 startScale =
            transform.localScale;

        while (elapsedTime < deathDuration)
        {
            elapsedTime += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedTime /
                    deathDuration
                );

            // 慢慢缩扁
            transform.localScale =
                new Vector3(
                    startScale.x,
                    Mathf.Lerp(
                        startScale.y,
                        startScale.y * 0.15f,
                        t
                    ),
                    startScale.z
                );

            // 慢慢透明
            if (spriteRenderer != null)
            {
                Color color =
                    originalColor;

                color.a =
                    Mathf.Lerp(
                        1f,
                        0f,
                        t
                    );

                spriteRenderer.color =
                    color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}