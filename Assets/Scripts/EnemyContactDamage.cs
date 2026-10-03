using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int contactDamage = 1;

    [Header("Player Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackUpForce = 3f;
    [SerializeField] private float hurtDuration = 0.25f;

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        PlayerHealth playerHealth =
            collision.gameObject
                .GetComponent<PlayerHealth>();

        PlayerController playerController =
            collision.gameObject
                .GetComponent<PlayerController>();

        if (playerHealth == null ||
            playerController == null)
        {
            return;
        }

        // 无敌时间内 TakeDamage 会返回 false
        bool damaged =
            playerHealth.TakeDamage(
                contactDamage
            );

        // 没有真的受伤就不击退、不硬直
        if (!damaged)
        {
            return;
        }

        // Player 在 Enemy 左边 → 往左弹
        // Player 在 Enemy 右边 → 往右弹
        float direction =
            collision.transform.position.x <
            transform.position.x
            ? -1f
            : 1f;

        Vector2 knockbackVelocity =
            new Vector2(
                direction *
                knockbackForce,

                knockbackUpForce
            );

        playerController
            .ApplyDamageKnockback(
                knockbackVelocity,
                hurtDuration
            );
    }
}