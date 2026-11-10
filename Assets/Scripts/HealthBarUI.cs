using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject barRoot;

    [SerializeField] private float visibleDuration = 2f;

    private Coroutine hideCoroutine;
    private int lastHealth;

    private void Start()
    {
        if (playerHealth == null)
        {
            return;
        }

        lastHealth = playerHealth.CurrentHealth;

        UpdateFill(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );

        if (barRoot != null)
        {
            barRoot.SetActive(false);
        }

        playerHealth.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(
        int currentHealth,
        int maxHealth)
    {
        UpdateFill(currentHealth, maxHealth);

        // 只有真正掉血时才显示
        if (currentHealth < lastHealth)
        {
            ShowHealthBar();
        }

        lastHealth = currentHealth;
    }

    private void UpdateFill(
        int currentHealth,
        int maxHealth)
    {
        if (fillImage == null || maxHealth <= 0)
        {
            return;
        }

        fillImage.fillAmount =
            (float)currentHealth / maxHealth;
    }

    private void ShowHealthBar()
    {
        if (barRoot == null)
        {
            return;
        }

        barRoot.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine =
            StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(
            visibleDuration
        );

        barRoot.SetActive(false);

        hideCoroutine = null;
    }
}