using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float flashInterval = 0.1f;

    private int currentHealth;
    private bool isInvincible;

    private SpriteRenderer spriteRenderer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDied;

    private Animator animator;
    private PlayerController playerController;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer =
            GetComponent<SpriteRenderer>();
        animator =
            GetComponent<Animator>();
        playerController =
            GetComponent<PlayerController>();
        rb =
            GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    public bool TakeDamage(int damage)
    {
        if (isInvincible ||
            currentHealth <= 0)
        {
            return false;
        }

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        Debug.Log(
            $"Player took {damage} damage. HP: {currentHealth}"
        );

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        StartCoroutine(
            Invincibility()
        );

        return true;
    }

    private IEnumerator Invincibility()
    {
        isInvincible = true;

        float elapsedTime = 0f;

        while (elapsedTime <
               invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled =
                    !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(
                flashInterval
            );

            elapsedTime +=
                flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player defeated.");

        // 禁止玩家继续操作
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 停止当前移动
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 通知 GameOverUI
        OnPlayerDied?.Invoke();

        // 不再 Destroy
        // 场景 1 秒后会自动重新加载
    }
}