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

    private bool bodyPushLocked = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
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
            // 如果正在进行普通身体推动，
            // 攻击 Knockback 优先
            if (bodyPushCoroutine != null)
            {
                StopCoroutine(bodyPushCoroutine);
                bodyPushCoroutine = null;
            }

            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
            }

            knockbackCoroutine =
                StartCoroutine(
                    Knockback(knockbackDirection)
                );
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(HitFlash());
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
        spriteRenderer.color = Color.red;

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
        // 攻击时暂时允许 Enemy 横向移动
        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity =
            new Vector2(
                knockbackDirection.x *
                knockbackForce,

                knockbackUpForce
            );

        yield return new WaitForSeconds(
            knockbackDuration
        );

        // 停止横向移动
        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );

        // 如果玩家还没有和 Enemy 拉开，攻击结束后继续锁住 X
        if (bodyPushLocked)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }

        knockbackCoroutine = null;
    }

    // -------------------------
    // Player 身体撞击时尝试轻微移动
    // -------------------------
    public void TryBodyPush(int direction)
    {
        if (rb == null ||
            bodyPushCoroutine != null ||
            knockbackCoroutine != null)
        {
            return;
        }

        // 从这次身体碰撞开始，Enemy 进入锁定状态
        bodyPushLocked = true;

        bodyPushCoroutine = StartCoroutine(
            BodyPush(direction)
        );
    }

    // -------------------------
    // 身体碰撞产生的小幅移动
    // -------------------------
    private IEnumerator BodyPush(int direction)
    {
        float startX = rb.position.x;

        float targetX =
            startX + direction * bodyPushDistance;

        float elapsedTime = 0f;

        // 推动期间允许横向移动
        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

        while (elapsedTime < bodyPushDuration)
        {
            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / bodyPushDuration
            );

            float newX = Mathf.Lerp(
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

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        // 推完以后暂时锁住 X
        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;

        bodyPushCoroutine = null;
    }

    public void ReleaseBodyPushLock()
    {
        if (rb == null || knockbackCoroutine != null)
        {
            return;
        }

        bodyPushLocked = false;

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;
    }

    private void Die()
    {
        Debug.Log(
            $"{gameObject.name} defeated."
        );

        Destroy(gameObject);
    }
}