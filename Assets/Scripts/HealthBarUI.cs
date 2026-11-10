using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject barRoot;

    [Header("Display")]
    [SerializeField] private float visibleDuration = 2f;

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration = 0.12f;

    [SerializeField] private Color normalColor =
        new Color32(217, 92, 92, 255);

    [SerializeField] private Color flashColor =
        Color.white;

    private Coroutine hideCoroutine;
    private Coroutine flashCoroutine;

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

        if (fillImage != null)
        {
            fillImage.color = normalColor;
        }

        if (barRoot != null)
        {
            barRoot.SetActive(false);
        }

        playerHealth.OnHealthChanged +=
            OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -=
                OnHealthChanged;
        }
    }

    private void OnHealthChanged(
        int currentHealth,
        int maxHealth)
    {
        UpdateFill(
            currentHealth,
            maxHealth
        );

        // 只有真正掉血时才显示并闪一下
        if (currentHealth < lastHealth)
        {
            ShowHealthBar();
            FlashHealthBar();
        }

        lastHealth = currentHealth;
    }

    private void UpdateFill(
        int currentHealth,
        int maxHealth)
    {
        if (fillImage == null ||
            maxHealth <= 0)
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
            StartCoroutine(
                HideAfterDelay()
            );
    }

    private void FlashHealthBar()
    {
        if (fillImage == null)
        {
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine =
            StartCoroutine(
                FlashRoutine()
            );
    }

    private IEnumerator FlashRoutine()
    {
        fillImage.color = flashColor;

        yield return new WaitForSeconds(
            flashDuration
        );

        fillImage.color = normalColor;

        flashCoroutine = null;
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