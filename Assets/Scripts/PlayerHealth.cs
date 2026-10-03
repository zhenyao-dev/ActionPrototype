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

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer =
            GetComponent<SpriteRenderer>();
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

        // 防止出现负数
        currentHealth =
            Mathf.Max(currentHealth, 0);

        // 扣血以后立刻通知 UI
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

        Destroy(gameObject);
    }
}