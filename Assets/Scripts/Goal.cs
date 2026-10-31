using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject levelClearText;

    private bool cleared;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D goalCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        goalCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!cleared)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }
    }

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

        // 隐藏钻石，但保留 Goal.cs 继续检测 R
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (goalCollider != null)
        {
            goalCollider.enabled = false;
        }
    }
}