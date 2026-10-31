using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject levelClearText;

    private bool cleared;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (cleared)
        {
            return;
        }

        PlayerController playerController =
            other.GetComponent<PlayerController>();

        if (playerController == null)
        {
            return;
        }

        cleared = true;

        // 停止玩家操作
        playerController.enabled = false;

        Rigidbody2D playerRb =
            other.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity =
                Vector2.zero;
        }

        // 显示通关文字
        if (levelClearText != null)
        {
            levelClearText.SetActive(true);
        }

        // 钻石消失
        gameObject.SetActive(false);
    }
}