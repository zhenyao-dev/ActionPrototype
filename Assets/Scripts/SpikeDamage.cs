using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Knockback")]
    [SerializeField] private float knockbackUpForce = 6f;
    [SerializeField] private float knockbackSideForce = 2f;
    [SerializeField] private float hurtDuration = 0.25f;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        PlayerController playerController =
            other.GetComponent<PlayerController>();

        if (playerHealth == null)
        {
            return;
        }

        // 只有真的成功扣血时才击退
        bool damaged =
            playerHealth.TakeDamage(damage);

        if (!damaged)
        {
            return;
        }

        if (playerController != null)
        {
            float direction =
                other.transform.position.x <
                transform.position.x
                    ? -1f
                    : 1f;

            playerController.ApplyDamageKnockback(
                new Vector2(
                    direction * knockbackSideForce,
                    knockbackUpForce
                ),
                hurtDuration
            );
        }
    }
}