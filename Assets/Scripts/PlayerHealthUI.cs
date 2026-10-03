using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (playerHealth == null ||
            healthText == null)
        {
            return;
        }

        playerHealth.OnHealthChanged +=
            UpdateHealthText;

        UpdateHealthText(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );
    }

    private void UpdateHealthText(
        int currentHealth,
        int maxHealth)
    {
        healthText.text =
            $"HP: {currentHealth} / {maxHealth}";
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -=
                UpdateHealthText;
        }
    }
}